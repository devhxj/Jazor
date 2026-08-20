namespace Jazor.CLR;

/// <summary>
/// CLR module for System.TimeOnly，映射成 JavaScript 中的 BigInt ticks（100ns）
/// </summary>
[ECMAScriptModule("System/TimeOnlyModule.js")]
[Jazor(Op.Alias, "System.TimeOnly", "Object")]
public static class TimeOnlyModule
{
	private static BigInt TicksPerDay => BigIntValue("864000000000");
	private static BigInt TicksPerHour => BigIntValue("36000000000");
	private static BigInt TicksPerMinute => BigIntValue("600000000");
	private static BigInt TicksPerSecond => BigIntValue("10000000");
	private static Number AllowedDateTimeStylesMask => 7;

	private static bool IsAsciiDigit(char value)
		=> value >= '0' && value <= '9';

	private static bool IsDigits(string text)
	{
		if (text.Length == 0)
			return false;

		for (var i = 0; i < text.Length; i++)
		{
			if (!IsAsciiDigit(text[i]))
				return false;
		}

		return true;
	}

	private static void ValidateTimeParts(Number hour, Number minute, Number second, Number millisecond, Number microsecond)
	{
		if (Math.FloorFunc(hour) != hour || Math.FloorFunc(minute) != minute || Math.FloorFunc(second) != second || Math.FloorFunc(millisecond) != millisecond || Math.FloorFunc(microsecond) != microsecond)
			throw new Error("ArgumentOutOfRangeException: TimeOnly components must be whole numbers.");
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999 || microsecond < 0 || microsecond > 999)
			throw new Error("ArgumentOutOfRangeException: One or more TimeOnly components are out of range.");
	}

	private static RuntimeModule.JTimeOnly CreateTimeOnly(Number hour, Number minute, Number second, Number millisecond, Number microsecond)
	{
		ValidateTimeParts(hour, minute, second, millisecond, microsecond);
		return new RuntimeModule.JTimeOnly(
			BigIntValue(hour) * TicksPerHour
			+ BigIntValue(minute) * TicksPerMinute
			+ BigIntValue(second) * TicksPerSecond
			+ BigIntValue(millisecond) * BigIntValue("10000")
			+ BigIntValue(microsecond) * BigIntValue(10));
	}

	private static BigInt CreateTruncatedTicksFromDouble(Number value)
	{
		if (DoubleModule.IsNaNCore(value))
			throw new Error("ArgumentException: Value cannot be NaN.");

		if (!DoubleModule.IsFiniteCore(value))
			throw new Error("ArgumentOutOfRangeException: Value must be finite.");

		return BigIntValue(Math.Trunc(value));
	}

	private static Array<object?> AddWithWrappedDays(RuntimeModule.JTimeOnly instance, BigInt deltaTicks)
	{
		var total = instance.Ticks + deltaTicks;
		var wrapped = NumberValue(total / TicksPerDay);
		var result = total % TicksPerDay;
		if (result < BigInt.Zero)
		{
			result += TicksPerDay;
			wrapped--;
		}

		return [new RuntimeModule.JTimeOnly(result), wrapped];
	}

	private static RuntimeModule.JTimeOnly CreateTimeOnlyFromTicks(BigInt ticks)
	{
		if (ticks < BigInt.Zero || ticks >= TicksPerDay)
			throw new Error("ArgumentOutOfRangeException: TimeOnly ticks must be within a single day.");

		return new RuntimeModule.JTimeOnly(ticks);
	}

	private static RuntimeModule.JTimeOnly ParseCore(string s)
	{
		var text = s.Trim();
		if (text.Length == 0)
			throw new Error("FormatException: String was not recognized as a valid TimeOnly.");

		var first = text.IndexOf(':');
		if (first < 0)
			throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");

		var second = text.IndexOf(':', first + 1);
		var hourText = text.Substring(0, first);
		var minuteText = second < 0 ? text.Substring(first + 1) : text.Substring(first + 1, second - first - 1);
		if (!IsDigits(hourText) || !IsDigits(minuteText))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");

		var hour = NumberValue(hourText);
		var minute = NumberValue(minuteText);
		var secondValue = 0;
		var fractionTicks = BigInt.Zero;

		if (second >= 0)
		{
			var fractionIndex = text.IndexOf('.', second + 1);
			var secondText = fractionIndex < 0 ? text.Substring(second + 1) : text.Substring(second + 1, fractionIndex - second - 1);
			if (!IsDigits(secondText))
				throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");

			secondValue = NumberValue(secondText);

			if (fractionIndex >= 0)
			{
				var fractionText = text.Substring(fractionIndex + 1);
				if (fractionText.Length == 0 || fractionText.Length > 7 || !IsDigits(fractionText))
					throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");

				while (fractionText.Length < 7)
					fractionText += "0";

				fractionTicks = BigIntValue(fractionText);
			}
		}

		if (IsNaN(hour) || IsNaN(minute) || IsNaN(secondValue))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || secondValue < 0 || secondValue > 59)
			throw new Error($"FormatException: String '{s}' was not recognized as a valid TimeOnly.");

		return new RuntimeModule.JTimeOnly(BigIntValue(hour) * TicksPerHour + BigIntValue(minute) * TicksPerMinute + BigIntValue(secondValue) * TicksPerSecond + fractionTicks);
	}

	private static Number GetDateTimeStylesValue(object style)
	{
		// DateTimeStyles 在当前 lowering 中会擦除为数值字面量。
		if (style is Number numberStyle)
			return numberStyle;
		if (style == null)
			return 0;

		throw new Error("ArgumentException: Invalid DateTimeStyles value.");
	}

	private static bool IsSupportedDateTimeStyles(Number style)
		=> style >= 0 && Math.FloorFunc(style) == style && (style & ~AllowedDateTimeStylesMask) == 0;

	[Jazor(Op.Import ,"System.TimeOnly.TimeOnly()")]
	public static RuntimeModule.JTimeOnly _9f78f92d0753f4cf() => new(BigInt.Zero);

	/// <summary>
	/// C#: TimeOnly.MinValue (00:00:00)
	/// JS: wrapper with zero ticks
	/// </summary>
	[Jazor(Op.Import, "static System.TimeOnly.MinValue.get")]
	public static RuntimeModule.JTimeOnly _5a02197e2ef2252f() => new(BigInt.Zero);

	/// <summary>
	/// C#: TimeOnly.MaxValue (23:59:59.9999999)
	/// JS: wrapper with max ticks
	/// </summary>
	[Jazor(Op.Import, "static System.TimeOnly.MaxValue.get")]
	public static RuntimeModule.JTimeOnly _b1d0e19d91dbb54a() => new(BigIntValue("863999999999"));

	/// <summary>
	/// C#: new TimeOnly(hour, minute)
	/// JS: hour * 36000000000n + minute * 600000000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.TimeOnly(int, int)")]
	public static RuntimeModule.JTimeOnly _62d395c56c4c299d(Number hour, Number minute)
		=> CreateTimeOnly(hour, minute, 0, 0, 0);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second)
	/// JS: hour * 36000000000n + minute * 600000000n + second * 10000000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.TimeOnly(int, int, int)")]
	public static RuntimeModule.JTimeOnly _e9a3481b3456aad4(Number hour, Number minute, Number second)
		=> CreateTimeOnly(hour, minute, second, 0, 0);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second, millisecond)
	/// JS: hour * 36000000000n + minute * 600000000n + second * 10000000n + millisecond * 10000n
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.TimeOnly(int, int, int, int)")]
	public static RuntimeModule.JTimeOnly _335167098e226ccf(Number hour, Number minute, Number second, Number millisecond)
		=> CreateTimeOnly(hour, minute, second, millisecond, 0);

	/// <summary>
	/// C#: new TimeOnly(hour, minute, second, millisecond, microsecond)
	/// JS: hour * 36000000000n + minute * 600000000n + second * 10000000n + millisecond * 10000n + microsecond * 10n
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.TimeOnly(int, int, int, int, int)")]
	public static RuntimeModule.JTimeOnly _28c8cb012fe0e547(Number hour, Number minute, Number second, Number millisecond, Number microsecond)
		=> CreateTimeOnly(hour, minute, second, millisecond, microsecond);

	/// <summary>
	/// C#: new TimeOnly(ticks)
	/// JS: ticks
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.TimeOnly(long)")]
	public static RuntimeModule.JTimeOnly _b8b3b95e8b848f44(BigInt ticks)
		=> CreateTimeOnlyFromTicks(ticks);

	/// <summary>
	/// C#: instance.Hour
	/// JS: Number((instance / 36000000000n) % 24n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Hour.get")]
	public static Number _201ef41481f4e3fb(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks / TicksPerHour) % BigIntValue(24));

	/// <summary>
	/// C#: instance.Minute
	/// JS: Number((instance / 600000000n) % 60n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Minute.get")]
	public static Number _009addd612610031(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks / TicksPerMinute) % BigIntValue(60));

	/// <summary>
	/// C#: instance.Second
	/// JS: Number((instance / 10000000n) % 60n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Second.get")]
	public static Number _b9481eedd6cbeb99(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks / TicksPerSecond) % BigIntValue(60));

	/// <summary>
	/// C#: instance.Millisecond
	/// JS: Number((instance / 10000n) % 1000n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Millisecond.get")]
	public static Number _3c789a48d39d0010(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks / BigIntValue(10000)) % BigIntValue(1000));

	/// <summary>
	/// C#: instance.Microsecond
	/// JS: Number((instance / 10n) % 1000n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Microsecond.get")]
	public static Number _a091b803b851e27e(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks / BigIntValue(10)) % BigIntValue(1000));

	/// <summary>
	/// C#: instance.Nanosecond
	/// JS: Number((instance % 10n) * 100n)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Nanosecond.get")]
	public static Number _656df0ee12e92399(RuntimeModule.JTimeOnly instance)
		=> NumberValue((instance.Ticks % BigIntValue(10)) * BigIntValue(100));

	/// <summary>
	/// C#: instance.Ticks
	/// JS: instance
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Ticks.get")]
	public static BigInt _2fd46050126234ac(RuntimeModule.JTimeOnly instance)
		=> instance.Ticks;

	/// <summary>
	/// C#: instance.Add(value)
	/// JS: normalize ticks to [0, 864000000000)
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Add(System.TimeSpan)")]
	public static RuntimeModule.JTimeOnly _4c935b985e7b6e02(RuntimeModule.JTimeOnly instance, RuntimeModule.JTimeSpan value)
	{
		return new RuntimeModule.JTimeOnly(instance.Ticks + value.Ticks);
	}

	/// <summary>
	/// C#: instance.Add(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.Add(System.TimeSpan, out int)")]
	public static Array<object?> _31bb07d031379025(RuntimeModule.JTimeOnly instance, RuntimeModule.JTimeSpan value, Number wrappedDays)
	{
		var total = instance.Ticks + value.Ticks;
		var wrapped = NumberValue(total / TicksPerDay);
		var result = total % TicksPerDay;
		if (result < BigInt.Zero)
		{
			result += TicksPerDay;
			wrapped--;
		}
		return [new RuntimeModule.JTimeOnly(result), wrapped];
	}

	/// <summary>
	/// C#: instance.AddHours(value)
	/// JS: normalize after adding hours
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddHours(double)")]
	public static RuntimeModule.JTimeOnly _8e71fa0d2695e84f(RuntimeModule.JTimeOnly instance, Number value)
	{
		var delta = new RuntimeModule.JTimeSpan(CreateTruncatedTicksFromDouble(value * 36000000000d));
		return _4c935b985e7b6e02(instance, delta);
	}

	/// <summary>
	/// C#: instance.AddHours(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddHours(double, out int)")]
	public static Array<object?> _ad6cad38823a5ef6(RuntimeModule.JTimeOnly instance, Number value, Number wrappedDays)
		=> AddWithWrappedDays(instance, CreateTruncatedTicksFromDouble(value * 36000000000d));

	/// <summary>
	/// C#: instance.AddMinutes(value)
	/// JS: normalize after adding minutes
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddMinutes(double)")]
	public static RuntimeModule.JTimeOnly _77bd7db30cbf3bc9(RuntimeModule.JTimeOnly instance, Number value)
	{
		var delta = new RuntimeModule.JTimeSpan(CreateTruncatedTicksFromDouble(value * 600000000d));
		return _4c935b985e7b6e02(instance, delta);
	}

	/// <summary>
	/// C#: instance.AddMinutes(value, out wrappedDays)
	/// JS: 返回 [result, wrappedDays]
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.AddMinutes(double, out int)")]
	public static Array<object?> _e698cb9920401887(RuntimeModule.JTimeOnly instance, Number value, Number wrappedDays)
		=> AddWithWrappedDays(instance, CreateTruncatedTicksFromDouble(value * 600000000d));

	/// <summary>
	/// C#: instance.IsBetween(start, end)
	/// JS: 支持跨午夜的范围检查
	/// </summary>
	[Jazor(Op.Import, "System.TimeOnly.IsBetween(System.TimeOnly, System.TimeOnly)")]
	public static bool _da64e8d379a7e47c(RuntimeModule.JTimeOnly instance, RuntimeModule.JTimeOnly start, RuntimeModule.JTimeOnly end)
		=> start.Ticks < end.Ticks
			? instance.Ticks >= start.Ticks && instance.Ticks < end.Ticks
			: instance.Ticks >= start.Ticks || instance.Ticks < end.Ticks;

	///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>are equal.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator ==(System.TimeOnly, System.TimeOnly)")]
	public static bool _8e47d4212be3070c(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks == right.Ticks;

	///<summary>Determines whether two specified instances of <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> are not equal.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator !=(System.TimeOnly, System.TimeOnly)")]
	public static bool _b3b712e75fff0050(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks != right.Ticks;

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator >(System.TimeOnly, System.TimeOnly)")]
	public static bool _341a3f0fbcda5677(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks > right.Ticks;

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or later than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator >=(System.TimeOnly, System.TimeOnly)")]
	public static bool _0656cf79f08fd69b(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks >= right.Ticks;

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> is earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator <(System.TimeOnly, System.TimeOnly)")]
	public static bool _9b001b8f9a72a57d(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks < right.Ticks;

	///<summary>Determines whether one specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> represents a time that is the same as or earlier than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref>.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator <=(System.TimeOnly, System.TimeOnly)")]
	public static bool _cd098f438100d4cb(RuntimeModule.JTimeOnly left, RuntimeModule.JTimeOnly right)
		=> left.Ticks <= right.Ticks;

	///<summary>Gives the elapsed time between two points on a circular clock, which will always be a positive value.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.operator -(System.TimeOnly, System.TimeOnly)")]
	public static RuntimeModule.JTimeSpan _888a9b439de5e7c1(RuntimeModule.JTimeOnly t1, RuntimeModule.JTimeOnly t2)
	{
		var diff = t1.Ticks - t2.Ticks;
		return new RuntimeModule.JTimeSpan(diff < BigInt.Zero ? diff + TicksPerDay : diff);
	}

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" /> and <see cref="P:System.TimeOnly.Minute" />.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.Deconstruct(out int, out int)")]
	public static Array<object?> _d6170153a1f10bc3(RuntimeModule.JTimeOnly instance, Number hour, Number minute)
		=> [_201ef41481f4e3fb(instance), _009addd612610031(instance)];

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, and <see cref="P:System.TimeOnly.Second" />.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.Deconstruct(out int, out int, out int)")]
	public static Array<object?> _d36793074735968e(RuntimeModule.JTimeOnly instance, Number hour, Number minute, Number second)
		=> [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance)];

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, and <see cref="P:System.TimeOnly.Millisecond" />.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.Deconstruct(out int, out int, out int, out int)")]
	public static Array<object?> _b349a5fd892d33be(RuntimeModule.JTimeOnly instance, Number hour, Number minute, Number second, Number millisecond)
		=> [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance)];

	///<summary>Deconstructs this <see cref="T:System.TimeOnly" /> instance into <see cref="P:System.TimeOnly.Hour" />, <see cref="P:System.TimeOnly.Minute" />, <see cref="P:System.TimeOnly.Second" />, <see cref="P:System.TimeOnly.Millisecond" />, and <see cref="P:System.TimeOnly.Microsecond" />.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.Deconstruct(out int, out int, out int, out int, out int)")]
	public static Array<object?> _1f5bb15cea73f15b(RuntimeModule.JTimeOnly instance, Number hour, Number minute, Number second, Number millisecond, Number microsecond)
		=> [_201ef41481f4e3fb(instance), _009addd612610031(instance), _b9481eedd6cbeb99(instance), _3c789a48d39d0010(instance), _a091b803b851e27e(instance)];

	///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a time span representing the time elapsed since midnight.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.FromTimeSpan(System.TimeSpan)")]
	public static RuntimeModule.JTimeOnly _df2fe8c100ae98f0(RuntimeModule.JTimeSpan timeSpan)
		=> CreateTimeOnlyFromTicks(timeSpan.Ticks);

	///<summary>Constructs a <see cref="T:System.TimeOnly" /> object from a <see cref="T:System.DateTime" /> representing the time of the day in this <see cref="T:System.DateTime" /> object.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.FromDateTime(System.DateTime)")]
	public static RuntimeModule.JTimeOnly _a305982aa6859677(RuntimeModule.JDateTime dateTime)
	{
		var milliseconds = (((dateTime.Date.GetHours() * 60 + dateTime.Date.GetMinutes()) * 60 + dateTime.Date.GetSeconds()) * 1000) + dateTime.Date.GetMilliseconds();
		return new RuntimeModule.JTimeOnly(BigIntValue(milliseconds) * BigIntValue("10000") + dateTime.SubMillisecondTicks);
	}

	///<summary>Convert the current <see cref="T:System.TimeOnly" /> instance to a <see cref="T:System.TimeSpan" /> object.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToTimeSpan()")]
	public static RuntimeModule.JTimeSpan _3ae6313d263b390f(RuntimeModule.JTimeOnly instance) => new(instance.Ticks);

	///<summary>Compares the value of this instance to a specified <see cref="T:System.TimeOnly" /> value and indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.CompareTo(System.TimeOnly)")]
	public static Number _b08fb6c2056f6cd2(RuntimeModule.JTimeOnly instance, RuntimeModule.JTimeOnly value)
	{
		if (instance.Ticks < value.Ticks)
			return -1;
		if (instance.Ticks > value.Ticks)
			return 1;
		return 0;
	}

	///<summary>Compares the value of this instance to a specified object that contains a specified <see cref="T:System.TimeOnly" /> value, and returns an integer that indicates whether this instance is earlier than, the same as, or later than the specified <see cref="T:System.TimeOnly" /> value.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.CompareTo(object)")]
	public static Number _fa5c092641b8d1d5(RuntimeModule.JTimeOnly instance, object? value)
	{
		if (value == null)
			return 1;

		var other = value as RuntimeModule.JTimeOnly;
		if (other == null)
			throw new Error("ArgumentException: Object must be of type TimeOnly.");

		return _b08fb6c2056f6cd2(instance, other);
	}

	///<summary>Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="T:System.TimeOnly" /> instance.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.Equals(System.TimeOnly)")]
	public static bool _f6e2f8f76d2b030d(RuntimeModule.JTimeOnly instance, RuntimeModule.JTimeOnly value)
		=> instance.Ticks == value.Ticks;

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Import ,"override System.TimeOnly.Equals(object)")]
	public static bool _f70c423884fcb611(RuntimeModule.JTimeOnly instance, object? value)
	{
		var other = value as RuntimeModule.JTimeOnly;
		return other != null && _f6e2f8f76d2b030d(instance, other);
	}

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import ,"override System.TimeOnly.GetHashCode()")]
	public static Number _ec44c7db9ffc5397(RuntimeModule.JTimeOnly instance)
		=> RuntimeModule.GetInt64HashCode(instance.Ticks);

	///<summary>Converts a memory span that contains string representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JTimeOnly _5c89b5211b528926(string s, Intl.NumberFormat? provider, object style)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			throw new Error("ArgumentException: The only supported DateTimeStyles values are AllowLeadingWhite, AllowTrailingWhite, AllowInnerWhite, and AllowWhiteSpaces.");

		return ParseCore(s);
	}

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JTimeOnly _7c5c52c213c7d2e0(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[])")]
	public extern static RuntimeModule.JTimeOnly _fe05a1ffa3020076(string s, object formats);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JTimeOnly _b22aa6d58a65860e(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using the conventions of the current culture.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.Parse(string)")]
	public static RuntimeModule.JTimeOnly _c2335ab7e556bf0b(string s)
		=> ParseCore(s);

	///<summary>Converts the string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent by using culture-specific format information and a formatting style.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.Parse(string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public static RuntimeModule.JTimeOnly _b10aeed232e37ce3(string s, Intl.NumberFormat? provider, object style)
		=> _5c89b5211b528926(s, provider, style);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string)")]
	public extern static RuntimeModule.JTimeOnly _716638d6af9e1f50(string s, string format);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JTimeOnly _464a80539f893705(string s, string format, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span to a <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string[])")]
	public extern static RuntimeModule.JTimeOnly _732d047579691da6(string s, object formats);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information, and style.            The format of the string representation must match at least one of the specified formats exactly or an exception is thrown.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.ParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles)")]
	public extern static RuntimeModule.JTimeOnly _a753be3cfd781575(string s, object formats, Intl.NumberFormat? provider, object style);

	///<summary>Converts the specified span representation of a time to its TimeOnly equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, out System.TimeOnly)")]
	public static Array<object?> _94c68599373e4134(string s, RuntimeModule.JTimeOnly result)
		=> _ee7de3e005ab6751(s, result);

	///<summary>Converts the specified span representation of a time to its <xref data-throw-if-not-resolved="true" uid="System.TimeOnly"></xref> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public static Array<object?> _33c24989822cc33a(string s, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			return [false, new RuntimeModule.JTimeOnly(BigInt.Zero)];

		return _ee7de3e005ab6751(s, result);
	}

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, out System.TimeOnly)")]
	public extern static Array<object?> _e2de5093ab6411a5(string s, string format, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _533e30052a71b943(string s, string format, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], out System.TimeOnly)")]
	public extern static Array<object?> _7949d623f32a801f(string s, object formats, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified character span of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _c88c8d59055208af(string s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(string, out System.TimeOnly)")]
	public static Array<object?> _ee7de3e005ab6751(string? s, RuntimeModule.JTimeOnly result)
	{
		if (s == null || s.Length == 0)
			return [false, new RuntimeModule.JTimeOnly(BigInt.Zero)];

		try
		{
			return [true, ParseCore(s)];
		}
		catch
		{
			return [false, new RuntimeModule.JTimeOnly(BigInt.Zero)];
		}
	}

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified array of formats, culture-specific format information and style, and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public static Array<object?> _c9d76d7d723eb7f2(string? s, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result)
	{
		var styleValue = GetDateTimeStylesValue(style);
		if (!IsSupportedDateTimeStyles(styleValue))
			return [false, new RuntimeModule.JTimeOnly(BigInt.Zero)];

		return _ee7de3e005ab6751(s, result);
	}

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string, out System.TimeOnly)")]
	public extern static Array<object?> _635f76a219a898ce(string? s, string? format, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified span representation of a time to its <see cref="T:System.TimeOnly" /> equivalent using the specified format, culture-specific format information, and style.            The format of the string representation must match the specified format exactly. The method returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string, System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _5d909e2eac7e90ea(string? s, string? format, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string[], out System.TimeOnly)")]
	public extern static Array<object?> _c464924dd070f03b(string? s, object formats, RuntimeModule.JTimeOnly result);

	///<summary>Converts the specified string representation of a time to its <see cref="T:System.TimeOnly" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
	[Jazor(Op.Discard ,"static System.TimeOnly.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.DateTimeStyles, out System.TimeOnly)")]
	public extern static Array<object?> _a8c2964fb6e24ce0(string? s, object formats, Intl.NumberFormat? provider, object style, RuntimeModule.JTimeOnly result);

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent long date string representation.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToLongTimeString()")]
	public static string _237d7e75836b3e58(RuntimeModule.JTimeOnly instance)
		=> instance.ToString();

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToShortTimeString()")]
	public static string _656ad6fcd28355ef(RuntimeModule.JTimeOnly instance)
		=> instance.ToString();

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent short time string representation using the formatting conventions of the current culture.</summary>
	[Jazor(Op.Alias ,"override System.TimeOnly.ToString()", "toString")]
	public extern static string _95a460669a453469(RuntimeModule.JTimeOnly instance);

	///<summary>Converts the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified format and the formatting conventions of the current culture.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToString(string)")]
	public static string _b95bf75d8e4cc6af(RuntimeModule.JTimeOnly instance, string? format)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToString(System.IFormatProvider)")]
	public static string _c2fe4568a7f1bbeb(RuntimeModule.JTimeOnly instance, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Converts the value of the current <see cref="T:System.TimeOnly" /> instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[Jazor(Op.Import ,"System.TimeOnly.ToString(string, System.IFormatProvider)")]
	public static string _dd80539f727e11c1(RuntimeModule.JTimeOnly instance, string? format, Intl.NumberFormat? provider)
		=> instance.ToString();

	///<summary>Tries to format the value of the current TimeOnly instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _d3c7ece118e478fa(RuntimeModule.JTimeOnly instance, Uint32Array destination, Number charsWritten, string format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"System.TimeOnly.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _98dcae3d77df54e1(RuntimeModule.JTimeOnly instance, Uint8Array utf8Destination, Number bytesWritten, string format, Intl.NumberFormat? provider);

	///<summary>Parses a string into a value.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.Parse(string, System.IFormatProvider)")]
	public static RuntimeModule.JTimeOnly _ef54bbdfdbe24915(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(string, System.IFormatProvider, out System.TimeOnly)")]
	public static Array<object?> _8fea7e8fcaae2f91(string? s, Intl.NumberFormat? provider, RuntimeModule.JTimeOnly result)
		=> _ee7de3e005ab6751(s, result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public static RuntimeModule.JTimeOnly _ae9862bc80a4bba9(string s, Intl.NumberFormat? provider)
		=> ParseCore(s);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Import ,"static System.TimeOnly.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeOnly)")]
	public static Array<object?> _1c2553fed0fac496(string s, Intl.NumberFormat? provider, RuntimeModule.JTimeOnly result)
		=> _ee7de3e005ab6751(s, result);
}
