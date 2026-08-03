namespace Jazor.CLR;

/// <summary>
/// 将 System.String 的常用比较、查找、切片、格式化和转换 API 映射到 JavaScript String。
/// </summary>
/// <remarks>
/// JavaScript String 与 .NET UTF-16 字符串在大多数索引操作上相近，但比较、格式化、culture
/// 和 null 行为并不自动等价；复杂路径使用 Import，短小稳定的路径才使用 Inline。
/// </remarks>
[ECMAScriptModule("System/StringModule.js")]
[Jazor(Op.Alias, "string","String")]
public static class StringModule
{
	[ECMAScriptInline("__arg1.normalize()")]
	private extern static string NormalizeDefault(string value);

	[ECMAScriptInline("__arg1.normalize(__arg2)")]
	private extern static string NormalizeWithForm(string value, string form);

	private static void EnsureNonNegativeWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value || value < 0)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a non-negative whole number.");
	}

	private static string JoinCharacters(Array<string> value, Number startIndex, Number length)
	{
		var result = "";
		for (var index = startIndex; index < startIndex + length; index++)
			result += value[index];
		return result;
	}

	private static void EnsureStringIndex(string value, Number index, string parameterName)
	{
		EnsureNonNegativeWholeNumber(index, parameterName);
		if (index > value.Length)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} is outside the string.");
	}

	private static Number CompareOrdinalRange(
		string? strA,
		Number indexA,
		string? strB,
		Number indexB,
		Number length)
	{
		if (strA == null)
			return strB == null ? 0 : -1;
		if (strB == null)
			return 1;

		EnsureStringIndex(strA, indexA, "indexA");
		EnsureStringIndex(strB, indexB, "indexB");
		EnsureNonNegativeWholeNumber(length, "length");

		var availableA = strA.Length - indexA;
		var availableB = strB.Length - indexB;
		var countA = length < availableA ? length : availableA;
		var countB = length < availableB ? length : availableB;
		var sharedCount = countA < countB ? countA : countB;
		for (var offset = 0; offset < sharedCount; offset++)
		{
			var difference = strA.CharCodeAt(indexA + offset) - strB.CharCodeAt(indexB + offset);
			if (difference != 0)
				return difference;
		}

		return countA - countB;
	}

	private static void CopyCharacters(
		string instance,
		Number sourceIndex,
		Array<string> destination,
		Number destinationIndex,
		Number count)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureStringIndex(instance, sourceIndex, "sourceIndex");
		if (destination == null)
			throw new Error("ArgumentNullException: destination is null.");
		EnsureNonNegativeWholeNumber(destinationIndex, "destinationIndex");
		EnsureNonNegativeWholeNumber(count, "count");
		if (sourceIndex > instance.Length - count)
			throw new Error("ArgumentOutOfRangeException: sourceIndex and count exceed the string.");
		if (destinationIndex > destination.Length - count)
			throw new Error("ArgumentException: destination array is too small.");

		for (var offset = 0; offset < count; offset++)
			destination[destinationIndex + offset] = instance[sourceIndex + offset].ToString();
	}

	private static string ConcatStrings(IEnumerable<string?> values, string separator, string parameterName)
	{
		if (values == null)
			throw new Error($"ArgumentNullException: {parameterName} is null.");

		var result = "";
		var first = true;
		foreach (var value in values)
		{
			if (!first)
				result += separator;
			result += value ?? "";
			first = false;
		}
		return result;
	}

	private static string ConcatStrings(Array<string?> values, string separator, string parameterName)
	{
		if (values == null)
			throw new Error($"ArgumentNullException: {parameterName} is null.");

		var result = "";
		for (var index = 0; index < values.Length; index++)
		{
			if (index != 0)
				result += separator;
			result += values[index] ?? "";
		}
		return result;
	}

	private static string ConcatValues<T>(IEnumerable<T> values, string separator, string parameterName)
	{
		if (values == null)
			throw new Error($"ArgumentNullException: {parameterName} is null.");

		var result = "";
		var first = true;
		foreach (var value in values)
		{
			if (!first)
				result += separator;
			result += RuntimeModule.GetStringRepresentation(value);
			first = false;
		}
		return result;
	}

	private static string ConcatValues<T>(Array<T> values, string separator, string parameterName)
	{
		if (values == null)
			throw new Error($"ArgumentNullException: {parameterName} is null.");

		var result = "";
		for (var index = 0; index < values.Length; index++)
		{
			if (index != 0)
				result += separator;
			result += RuntimeModule.GetStringRepresentation(values[index]);
		}
		return result;
	}

	private static string JoinRange(string separator, Array<string?> value, Number startIndex, Number count)
	{
		if (value == null)
			throw new Error("ArgumentNullException: value is null.");
		EnsureNonNegativeWholeNumber(startIndex, nameof(startIndex));
		EnsureNonNegativeWholeNumber(count, nameof(count));
		if (startIndex > value.Length - count)
			throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");

		var result = "";
		for (var index = startIndex; index < startIndex + count; index++)
		{
			if (index != startIndex)
				result += separator;
			result += value[index] ?? "";
		}
		return result;
	}

	private static string GetNormalizationForm(Number normalizationForm)
	{
		if (normalizationForm == 1)
			return "NFC";
		if (normalizationForm == 2)
			return "NFD";
		if (normalizationForm == 5)
			return "NFKC";
		if (normalizationForm == 6)
			return "NFKD";
		throw new Error("ArgumentException: Invalid normalization form.");
	}

	private static string ReplaceLineEndingsCore(string instance, string replacementText)
	{
		if (replacementText == null)
			throw new Error("ArgumentNullException: replacementText is null.");

		var result = "";
		var segmentStart = 0;
		for (var index = 0; index < instance.Length; index++)
		{
			var codeUnit = instance.CharCodeAt(index);
			var isCarriageReturn = codeUnit == 13;
			var isLineEnding = isCarriageReturn
				|| codeUnit == 10
				|| codeUnit == 12
				|| codeUnit == 133
				|| codeUnit == 8232
				|| codeUnit == 8233;
			if (!isLineEnding)
				continue;

			result += instance.Substring(segmentStart, index - segmentStart);
			result += replacementText;
			if (isCarriageReturn && index + 1 < instance.Length && instance.CharCodeAt(index + 1) == 10)
				index++;
			segmentStart = index + 1;
		}

		return segmentStart == 0 ? instance : result + instance.Substring(segmentStart);
	}

	private static string TrimReadOnlyCharacterSpan(
		string instance,
		RuntimeModule.JReadOnlyCharSpan trimChars,
		bool trimStart,
		bool trimEnd)
	{
		var characters = RuntimeModule.MaterializeReadOnlyCharSpan(trimChars);
		// Span overloads treat an empty character set as "trim nothing". The char[] overload
		// intentionally has different CLR semantics and falls back to whitespace trimming.
		if (characters.Length == 0)
			return instance;

		return TrimCharacterSet(instance, NormalizeCharSet(characters), trimStart, trimEnd);
	}

	private static string GetInternedString(string? value)
	{
		if (value == null)
			throw new Error("ArgumentNullException: str is null.");

		// String carriers are immutable JS primitives. Their CLR reference identity is intentionally
		// erased, so Intern cannot add an observable behavior to the supported carrier.
		// IsInterned remains unsupported because its null result exposes the intern-table state.
		return value;
	}

	///<summary>Represents the empty string. This field is read-only.</summary>
	[Jazor(Op.Inline, "static readonly string.Empty", "\"\"")]
	public extern static string _b16f79dc7b155be3();

	///<summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
	[Jazor(Op.Import ,"static string.Intern(string)")]
	public static string _1234444e218b96c3(string? str)
		=> GetInternedString(str);

	///<summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"static string.IsInterned(string)")]
	public extern static string? _0af8a50f6d6b3e26(string str);

	/// <summary>
	/// C#: string.Compare(string, string)
	/// JS: strA < strB ? -1 : (strA > strB ? 1 : 0)
	/// </summary>
	[Jazor(Op.Import, "static string.Compare(string, string)")]
	public static Number _e16eea9fe3891a62(string? strA, string? strB)
	{
		if (strA == null && strB == null) return 0;
		if (strA == null) return -1;
		if (strB == null) return 1;
		if (strA < strB) return -1;
		if (strA > strB) return 1;
		return 0;
	}

	/// <summary>
	/// C#: string.Compare(string, string, bool ignoreCase)
	/// JS: 使用 toLowerCase() 进行不区分大小写比较
	/// </summary>
	[Jazor(Op.Import, "static string.Compare(string, string, bool)")]
	public static Number _20874c0b43640318(string? strA, string? strB, bool ignoreCase)
	{
		if (strA == null && strB == null) return 0;
		if (strA == null) return -1;
		if (strB == null) return 1;
		var a = ignoreCase ? strA.ToLower() : strA;
		var b = ignoreCase ? strB.ToLower() : strB;
		if (a < b) return -1;
		if (a > b) return 1;
		return 0;
	}

	///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Import ,"static string.Compare(string, string, System.StringComparison)")]
	public static Number _9d940114ace1198f(string? strA, string? strB, object comparisonType)
		=> _20874c0b43640318(strA, strB, IsOrdinalIgnoreCase(comparisonType));

	///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, string, System.Globalization.CultureInfo, System.Globalization.CompareOptions)")]
	public extern static Number _3df4c7373f0b47b6(string? strA, string? strB, String? culture, object options);

	///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, string, bool, System.Globalization.CultureInfo)")]
	public extern static Number _7349ec2403e9750d(string? strA, string? strB, bool ignoreCase, String? culture);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, int, string, int, int)")]
	public extern static Number _27da56ab23a965a9(string? strA, Number indexA, string? strB, Number indexB, Number length);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, int, string, int, int, bool)")]
	public extern static Number _ae9588dc995de641(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, int, string, int, int, bool, System.Globalization.CultureInfo)")]
	public extern static Number _e926c87c90eaf4a5(string? strA, Number indexA, string? strB, Number indexB, Number length, bool ignoreCase, String? culture);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two substrings to each other in the sort order.</summary>
	[Jazor(Op.Discard ,"static string.Compare(string, int, string, int, int, System.Globalization.CultureInfo, System.Globalization.CompareOptions)")]
	public extern static Number _6de73d4e145d51a4(string? strA, Number indexA, string? strB, Number indexB, Number length, String? culture, object options);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	[Jazor(Op.Import ,"static string.Compare(string, int, string, int, int, System.StringComparison)")]
	public static Number _d78fb9d76fca75e4(string? strA, Number indexA, string? strB, Number indexB, Number length, object comparisonType)
	{
		var sliceA = SliceOrEmpty(strA, indexA, length);
		var sliceB = SliceOrEmpty(strB, indexB, length);
		return _20874c0b43640318(sliceA, sliceB, IsOrdinalIgnoreCase(comparisonType));
	}

	///<summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
	[Jazor(Op.Import ,"static string.CompareOrdinal(string, string)")]
	public static Number _a55d307de6e31c7b(string? strA, string? strB)
		=> _e16eea9fe3891a62(strA, strB);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring.</summary>
	[Jazor(Op.Import ,"static string.CompareOrdinal(string, int, string, int, int)")]
	public static Number _dc789454b6ef6bcb(string? strA, Number indexA, string? strB, Number indexB, Number length)
		=> CompareOrdinalRange(strA, indexA, strB, indexB, length);

	///<summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	[Jazor(Op.Import ,"string.CompareTo(object)")]
	public static Number _629b0613344d82e7(string instance, object? value)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		if (value == null)
			return 1;
		if (TypeOf(value) != "string")
			throw new Error("ArgumentException: Object must be of type String.");

		return _380e7c7649d703f0(instance, (string)value);
	}

	///<summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified string.</summary>
	[Jazor(Op.Import ,"string.CompareTo(string)")]
	public static Number _380e7c7649d703f0(string instance, string? strB)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		if (strB == null)
			return 1;
		if (instance < strB)
			return -1;
		if (instance > strB)
			return 1;
		return 0;
	}

	/// <summary>
	/// C#: str.EndsWith(value)
	/// JS: str.endsWith(value)
	/// </summary>
	[Jazor(Op.Alias, "string.EndsWith(string)", "endsWith")]
	public extern static bool _33de316681320ec7(string instance, string value);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[Jazor(Op.Import ,"string.EndsWith(string, System.StringComparison)")]
	public static bool _946b7129a48c8114(string instance, string value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().EndsWith(value.ToLower())
			: instance.EndsWith(value);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
	[Jazor(Op.Discard ,"string.EndsWith(string, bool, System.Globalization.CultureInfo)")]
	public extern static bool _679207cac049d3c6(string instance, string value, bool ignoreCase, String? culture);

	/// <summary>
	/// C#: str.EndsWith(value)
	/// JS: str.endsWith(value)
	/// </summary>
	[Jazor(Op.Alias, "string.EndsWith(char)", "endsWith")]
	public extern static bool _7619ce4eda48c8e8(string instance, Number value);

	///<summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
	[Jazor(Op.Inline ,"override string.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _def18c2802a57249(string instance, object? obj);

	///<summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
	[Jazor(Op.Inline ,"string.Equals(string)", "(__arg1 === __arg2)")]
	public extern static bool _6ee9bc86e4384225(string instance, string? value);

	///<summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[Jazor(Op.Import ,"string.Equals(string, System.StringComparison)")]
	public static bool _f8e1e01e8c17e8bb(string instance, string? value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? (instance?.ToLower() == value?.ToLower())
			: instance == value;

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
	[Jazor(Op.Inline ,"static string.Equals(string, string)", "(__arg1 === __arg2)")]
	public extern static bool _e6b1648151c863d5(string? a, string? b);

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[Jazor(Op.Import ,"static string.Equals(string, string, System.StringComparison)")]
	public static bool _b7c36408f0f172e9(string? a, string? b, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? (a?.ToLower() == b?.ToLower())
			: a == b;

	///<summary>Determines whether two specified strings have the same value.</summary>
	[Jazor(Op.Allowed ,"static string.operator ==(string, string)")]
	public extern static bool _ee27dec45b308755(string? a, string? b);

	///<summary>Determines whether two specified strings have different values.</summary>
	[Jazor(Op.Allowed ,"static string.operator !=(string, string)")]
	public extern static bool _1573803c425863d3(string? a, string? b);

	///<summary>Returns the hash code for this string.</summary>
	[Jazor(Op.Import, "override string.GetHashCode()")]
	public static Number _bccdd3f386a6fbbc(string instance)
		=> EqualityComparerT1Module<string>.GetHashCodeCore(instance);

	///<summary>Returns the hash code for this string using the specified rules.</summary>
	[Jazor(Op.Import ,"string.GetHashCode(System.StringComparison)")]
	public static Number _04edfc3090710ca7(string instance, Number comparisonType)
	{
		EnsureOrdinalHashComparison(comparisonType);
		return EqualityComparerT1Module<string>.GetHashCodeCore(instance);
	}

	///<summary>Returns the hash code for the provided read-only character span.</summary>
	[Jazor(Op.Import, "static string.GetHashCode(System.ReadOnlySpan<char>)")]
	public static Number _4598a18be32f839d(RuntimeModule.JReadOnlyCharSpan value)
		=> EqualityComparerT1Module<string>.GetHashCodeCore(RuntimeModule.MaterializeReadOnlyCharSpan(value));

	///<summary>Returns the hash code for the provided read-only character span using the specified rules.</summary>
	[Jazor(Op.Import ,"static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)")]
	public static Number _d123047f69d911f5(RuntimeModule.JReadOnlyCharSpan value, Number comparisonType)
	{
		EnsureOrdinalHashComparison(comparisonType);
		return EqualityComparerT1Module<string>.GetHashCodeCore(RuntimeModule.MaterializeReadOnlyCharSpan(value));
	}

	private static bool IsOrdinalIgnoreCase(object comparisonType)
		=> comparisonType is Number value && value == 5;

	private static void EnsureOrdinalHashComparison(Number comparisonType)
	{
		if (comparisonType == 4)
			return;

		if (comparisonType >= 0 && comparisonType <= 5)
			throw new Error("NotSupportedException: string hash comparison currently supports only StringComparison.Ordinal.");

		throw new Error("ArgumentException: comparisonType is not a valid StringComparison value.");
	}

	private static string SliceOrEmpty(string? value, Number start, Number length)
	{
		if (string.IsNullOrEmpty(value))
			return value ?? "";

		if (start >= value.Length || length <= 0)
			return "";

		var available = value.Length - start;
		var take = length < available ? length : available;
		return value.Substring(start, take);
	}

	/// <summary>
	/// C#: str.StartsWith(value)
	/// JS: str.startsWith(value)
	/// </summary>
	[Jazor(Op.Alias, "string.StartsWith(string)", "startsWith")]
	public extern static bool _1cda198f8257d023(string instance, string value);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[Jazor(Op.Import ,"string.StartsWith(string, System.StringComparison)")]
	public static bool _0333a0fd5f67d8a0(string instance, string value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().StartsWith(value.ToLower())
			: instance.StartsWith(value);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
	[Jazor(Op.Discard ,"string.StartsWith(string, bool, System.Globalization.CultureInfo)")]
	public extern static bool _16d66a076936ebd2(string instance, string value, bool ignoreCase, String? culture);

	/// <summary>
	/// C#: str.StartsWith(value)
	/// JS: str.startsWith(value)
	/// </summary>
	[Jazor(Op.Alias, "string.StartsWith(char)", "startsWith")]
	public extern static bool _ef46304ffa6d6ccf(string instance, Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified character array.</summary>
	[Jazor(Op.Import ,"string.String(char[])")]
	public static string _6651b0a853e8e991(Array<string> value)
	{
		if (value == null)
			throw new Error("ArgumentNullException: value is null.");
		return JoinCharacters(value, 0, value.Length);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
	[Jazor(Op.Import ,"string.String(char[], int, int)")]
	public static string _ddce1a944159fc8b(Array<string> value, Number startIndex, Number length)
	{
		if (value == null)
			throw new Error("ArgumentNullException: value is null.");
		EnsureNonNegativeWholeNumber(startIndex, nameof(startIndex));
		EnsureNonNegativeWholeNumber(length, nameof(length));
		if (startIndex > value.Length - length)
			throw new Error("ArgumentOutOfRangeException: startIndex and length must identify a valid range.");
		return JoinCharacters(value, startIndex, length);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
	[Jazor(Op.Import ,"string.String(char, int)")]
	public static string _0ce0d88e18c041c8(string c, Number count)
	{
		EnsureNonNegativeWholeNumber(count, nameof(count));
		return c.Repeat(count);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified read-only span.</summary>
	[Jazor(Op.Import, "string.String(System.ReadOnlySpan<char>)")]
	public static string _009fee2e166a416d(RuntimeModule.JReadOnlyCharSpan value)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(value);

	///<summary>Creates a new string with a specific length and initializes it after creation by using the specified callback.</summary>
	[Jazor(Op.Discard ,"static string.Create<TState>(int, TState, System.Buffers.SpanAction<char, TState>)")]
	public extern static string _dcfb926861070414<TState>(Number length, object state, object action);

	///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
	[Jazor(Op.Discard ,"static string.Create(System.IFormatProvider, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)")]
	public extern static Array<object?> _af610a42747a747c(Intl.NumberFormat? provider, object handler);

	///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
	[Jazor(Op.Discard ,"static string.Create(System.IFormatProvider, System.Span<char>, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)")]
	public extern static Array<object?> _1978314137f5a599(Intl.NumberFormat? provider, Uint32Array initialBuffer, object handler);

	///<summary>Defines an implicit conversion of a given string to a read-only span of characters.</summary>
	[Jazor(Op.Import, "static string.implicit operator System.ReadOnlySpan<char>(string)")]
	public static RuntimeModule.JReadOnlyCharSpan _5ff800b094791eb0(string? value)
		=> value ?? "";

	///<summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
	[Jazor(Op.Inline ,"string.Clone()", "__arg1")]
	public extern static object _488d7e5ec582c6fb(string instance);

	///<summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
	[Jazor(Op.Import ,"static string.Copy(string)")]
	public static string _0dc0a16fd99401f8(string str)
	{
		if (str == null)
			throw new Error("ArgumentNullException: str is null.");

		// Strings are immutable value carriers in the JavaScript target, so a distinct allocation
		// cannot be observed through the supported runtime surface.
		return str;
	}

	///<summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
	[Jazor(Op.Import ,"string.CopyTo(int, char[], int, int)")]
	public static void _45bb6097c28a2f1e(
		string instance,
		Number sourceIndex,
		Array<string> destination,
		Number destinationIndex,
		Number count)
		=> CopyCharacters(instance, sourceIndex, destination, destinationIndex, count);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[Jazor(Op.Discard ,"string.CopyTo(System.Span<char>)")]
	public extern static void _2b86529e4a090aee(string instance, Uint32Array destination);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[Jazor(Op.Discard ,"string.TryCopyTo(System.Span<char>)")]
	public extern static bool _b0ab2eeef447828c(string instance, Uint32Array destination);

	/// <summary>
	/// C#: str.ToCharArray()
	/// JS: str.split("")
	/// </summary>
	[Jazor(Op.Inline, "string.ToCharArray()", "__arg1.split(\"\")")]
	public extern static char[] _7b8eb7b3d52c463d(string instance);

	/// <summary>
	/// C#: str.ToCharArray(startIndex, length)
	/// JS: str.substring(startIndex, startIndex + length).split("")
	/// </summary>
	[Jazor(Op.Inline, "string.ToCharArray(int, int)", "__arg1.substring(__arg2, __arg2 + __arg3).split(\"\")")]
	public extern static char[] _53042938adf57f41(string instance, Number startIndex, Number length);

	/// <summary>
	/// C#: string.IsNullOrEmpty(value)
	/// JS: !value
	/// </summary>
	[Jazor(Op.Inline, "static string.IsNullOrEmpty(string)", "!__arg1")]
	public extern static bool _f6e1cc63ac93e98f(string? value);

	/// <summary>
	/// C#: string.IsNullOrWhiteSpace(value)
	/// JS: !value?.trim()
	/// </summary>
	[Jazor(Op.Inline, "static string.IsNullOrWhiteSpace(string)", "!__arg1?.trim()")]
	public extern static bool _257a1a64b4d0f7d2(string? value);

	///<summary>Returns a reference to the element of the string at index zero.This method is intended to support .NET compilers and is not intended to be called by user code.</summary>
	[Jazor(Op.Discard ,"string.GetPinnableReference()")]
	public extern static Number _519728f02e3ba627(string instance);

	/// <summary>
	/// C#: str.ToString()
	/// JS: str (no-op, string already a string)
	/// </summary>
	[Jazor(Op.Allowed, "override string.ToString()")]
	public extern static string _3158320a4854cc16(string instance);

	///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	[Jazor(Op.Inline ,"string.ToString(System.IFormatProvider)", "__arg1")]
	public extern static string _555baf594c383de9(string instance, Intl.NumberFormat? provider);

	///<summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
	[Jazor(Op.Discard ,"string.GetEnumerator()")]
	public extern static System.CharEnumerator _b5d8c191b0b746ca(string instance);

	///<summary>Returns an enumeration of <see cref="T:System.Text.Rune" /> from this string.</summary>
	[Jazor(Op.Discard ,"string.EnumerateRunes()")]
	public extern static System.Text.StringRuneEnumerator _1e33e6a38a2179d0(string instance);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for the <see cref="T:System.String" /> class.</summary>
	[Jazor(Op.Inline ,"string.GetTypeCode()", "18")]
	public extern static System.TypeCode _b4f593c93e2f2c61(string instance);

	///<summary>Indicates whether this string is in Unicode normalization form C.</summary>
	[Jazor(Op.Import ,"string.IsNormalized()")]
	public static bool _f645a0207f41fd4a(string instance)
		=> instance == NormalizeDefault(instance);

	///<summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
	[Jazor(Op.Import ,"string.IsNormalized(System.Text.NormalizationForm)")]
	public static bool _30d0ce62702ae938(string instance, Number normalizationForm)
		=> instance == NormalizeWithForm(instance, GetNormalizationForm(normalizationForm));

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
	[Jazor(Op.Import ,"string.Normalize()")]
	public static string _967ef647d59f3e39(string instance)
		=> NormalizeDefault(instance);

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
	[Jazor(Op.Import ,"string.Normalize(System.Text.NormalizationForm)")]
	public static string _59b116010f03241b(string instance, Number normalizationForm)
		=> NormalizeWithForm(instance, GetNormalizationForm(normalizationForm));

	/// <summary>
	/// C#: str[index]
	/// JS: str[index] (越界时抛出 IndexOutOfRangeException)
	/// 当前仍保留 Import：第一阶段 Compile contract 还没有统一的 throw-expression / IIFE 约定，
	/// 这里如果硬迁移，只会把越界分支重新编码成更脆弱的表达式技巧。
	/// </summary>
	[Jazor(Op.Import, "string.this[int].get")]
	public static string _5ad63706a889c294(string instance, Number index)
	{
		if (index < 0 || index >= instance.Length)
			throw new Error("IndexOutOfRangeException: index is out of range.");
		return instance.CharAt(index);
	}

	/// <summary>
	/// C#: str.Length
	/// JS: str.length
	/// </summary>
	[Jazor(Op.Alias, "string.Length.get", "length")]
	public extern static Number _1b0d64005dc28838(string instance);

	///<summary>Creates the string  representation of a specified object.</summary>
	[Jazor(Op.Import ,"static string.Concat(object)")]
	public static string _db938b9c2eb90d32(object? arg0)
		=> RuntimeModule.GetStringRepresentation(arg0);

	///<summary>Concatenates the string representations of two specified objects.</summary>
	[Jazor(Op.Import ,"static string.Concat(object, object)")]
	public static string _d330ca25546acf36(object? arg0, object? arg1)
		=> RuntimeModule.GetStringRepresentation(arg0) + RuntimeModule.GetStringRepresentation(arg1);

	///<summary>Concatenates the string representations of three specified objects.</summary>
	[Jazor(Op.Import ,"static string.Concat(object, object, object)")]
	public static string _dab9155adbef8f67(object? arg0, object? arg1, object? arg2)
		=> RuntimeModule.GetStringRepresentation(arg0)
			+ RuntimeModule.GetStringRepresentation(arg1)
			+ RuntimeModule.GetStringRepresentation(arg2);

	///<summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
	[Jazor(Op.Import ,"static string.Concat(params object[])")]
	public static string _e102498b82e5b869(Array<object?> args)
		=> ConcatValues(args, "", "args");

	///<summary>Concatenates the string representations of the elements in a specified span of objects.</summary>
	[Jazor(Op.Import, "static string.Concat(params System.ReadOnlySpan<object>)")]
	public static string _2d6a291b64a11ba3(Array<object?> args)
		=> ConcatValues(args, "", "args");

	///<summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
	[Jazor(Op.Import ,"static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)")]
	public static string _68574aee669f440f<T>(IEnumerable<T> values)
		=> ConcatValues(values, "", "values");

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
	[Jazor(Op.Import ,"static string.Concat(System.Collections.Generic.IEnumerable<string>)")]
	public static string _a2a66aa54427416c(IEnumerable<string?> values)
		=> ConcatStrings(values, "", "values");

	/// <summary>
	/// C#: string.Concat(str0, str1)
	/// JS: str0 + str1
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string)", "((__arg1 ?? \"\") + (__arg2 ?? \"\"))")]
	public extern static string _021d71ef80d7918e(string? str0, string? str1);

	/// <summary>
	/// C#: string.Concat(str0, str1, str2)
	/// JS: str0 + str1 + str2
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string, string)", "((__arg1 ?? \"\") + (__arg2 ?? \"\") + (__arg3 ?? \"\"))")]
	public extern static string _ccc7897cb6f89406(string? str0, string? str1, string? str2);

	/// <summary>
	/// C#: string.Concat(str0, str1, str2, str3)
	/// JS: str0 + str1 + str2 + str3
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string, string, string)", "((__arg1 ?? \"\") + (__arg2 ?? \"\") + (__arg3 ?? \"\") + (__arg4 ?? \"\"))")]
	public extern static string _abe4ba2b38df2f54(string? str0, string? str1, string? str2, string? str3);

	///<summary>Concatenates the string representations of two specified read-only character spans.</summary>
	[Jazor(Op.Import, "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public static string _a6102c27abe1ff18(RuntimeModule.JReadOnlyCharSpan str0, RuntimeModule.JReadOnlyCharSpan str1)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(str0) + RuntimeModule.MaterializeReadOnlyCharSpan(str1);

	///<summary>Concatenates the string representations of three specified read-only character spans.</summary>
	[Jazor(Op.Import, "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public static string _7de0cfb062a343ee(
		RuntimeModule.JReadOnlyCharSpan str0,
		RuntimeModule.JReadOnlyCharSpan str1,
		RuntimeModule.JReadOnlyCharSpan str2)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(str0) +
			RuntimeModule.MaterializeReadOnlyCharSpan(str1) +
			RuntimeModule.MaterializeReadOnlyCharSpan(str2);

	///<summary>Concatenates the string representations of four specified read-only character spans.</summary>
	[Jazor(Op.Import, "static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public static string _5177ae056c5ca775(
		RuntimeModule.JReadOnlyCharSpan str0,
		RuntimeModule.JReadOnlyCharSpan str1,
		RuntimeModule.JReadOnlyCharSpan str2,
		RuntimeModule.JReadOnlyCharSpan str3)
		=> RuntimeModule.MaterializeReadOnlyCharSpan(str0) +
			RuntimeModule.MaterializeReadOnlyCharSpan(str1) +
			RuntimeModule.MaterializeReadOnlyCharSpan(str2) +
			RuntimeModule.MaterializeReadOnlyCharSpan(str3);

	///<summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
	[Jazor(Op.Import ,"static string.Concat(params string[])")]
	public static string _0f681227152a171b(Array<string?> values)
		=> ConcatStrings(values, "", "values");

	///<summary>Concatenates the elements of a specified span of <see cref="T:System.String" />.</summary>
	[Jazor(Op.Import ,"static string.Concat(params System.ReadOnlySpan<string>)")]
	public static string _22098d7fa5ce7a81(Array<string?> values)
		=> ConcatStrings(values, "", "values");

	/// <summary>
	/// C#: string.Format(string, object)
	/// JS: format.replace(/\{0\}/g, arg0)
	/// </summary>
	[Jazor(Op.Import, "static string.Format(string, object)")]
	public static string _980dff69bc3b8afa(string format, object? arg0)
	{
		if (format == null)
			throw new Error("ArgumentNullException: Format string cannot be null.");
		return format.Replace("{0}", arg0?.ToString() ?? "");
	}

	/// <summary>
	/// C#: string.Format(string, object, object)
	/// JS: format.replace(/\{0\}/g, arg0).replace(/\{1\}/g, arg1)
	/// </summary>
	[Jazor(Op.Import, "static string.Format(string, object, object)")]
	public static string _8606f3cc36d1f8ed(string format, object? arg0, object? arg1)
	{
		if (format == null)
			throw new Error("ArgumentNullException: Format string cannot be null.");
		return format
			.Replace("{0}", arg0?.ToString() ?? "")
			.Replace("{1}", arg1?.ToString() ?? "");
	}

	/// <summary>
	/// C#: string.Format(string, object, object, object)
	/// JS: format.replace for {0}, {1}, {2}
	/// </summary>
	[Jazor(Op.Import, "static string.Format(string, object, object, object)")]
	public static string _cda0978188193522(string format, object? arg0, object? arg1, object? arg2)
	{
		if (format == null)
			throw new Error("ArgumentNullException: Format string cannot be null.");
		return format
			.Replace("{0}", arg0?.ToString() ?? "")
			.Replace("{1}", arg1?.ToString() ?? "")
			.Replace("{2}", arg2?.ToString() ?? "");
	}

	/// <summary>
	/// C#: string.Format(string, params object[])
	/// JS: 使用正则表达式替换所有 {N} 占位符
	/// </summary>
	[Jazor(Op.Import, "static string.Format(string, params object[])")]
	public static string _99b8bed2ce27774c(string format, Array<object?> args)
	{
		if (format == null)
			throw new Error("ArgumentNullException: Format string cannot be null.");
		var result = format;
		for (uint i = 0; i < args.Length; i++)
		{
			result = result.Replace("{" + i + "}", args[i]?.ToString() ?? "");
		}
		return result;
	}

	///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified span.</summary>
	[Jazor(Op.Discard ,"static string.Format(string, params System.ReadOnlySpan<object>)")]
	public extern static string _38dfe358e33e2c5d(string format,  object args);

	///<summary>Replaces the format item or items in a specified string with the string representation of the corresponding object. A parameter supplies culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, string, object)")]
	public extern static string _03246c01949cf478(Intl.NumberFormat? provider, string format, object? arg0);

	///<summary>Replaces the format items in a string with the string representation of two specified objects. A parameter supplies culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, string, object, object)")]
	public extern static string _661214177662ec13(Intl.NumberFormat? provider, string format, object? arg0, object? arg1);

	///<summary>Replaces the format items in a string with the string representation of three specified objects. An parameter supplies culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, string, object, object, object)")]
	public extern static string _915cdc23ed4c4425(Intl.NumberFormat? provider, string format, object? arg0, object? arg1, object? arg2);

	///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, string, params object[])")]
	public extern static string _2b199e5bf9c94fc2(Intl.NumberFormat? provider, string format,  object args);

	///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified span. A parameter supplies culture-specific formatting information.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, string, params System.ReadOnlySpan<object>)")]
	public extern static string _8a09a1f92212621f(Intl.NumberFormat? provider, string format,  object args);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[Jazor(Op.Discard ,"static string.Format<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)")]
	public extern static string _2fd17baa6bc57571<TArg0>(Intl.NumberFormat? provider, object format, object arg0);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[Jazor(Op.Discard ,"static string.Format<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)")]
	public extern static string _879b6befd667cd5c<TArg0, TArg1>(Intl.NumberFormat? provider, object format, object arg0, object arg1);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[Jazor(Op.Discard ,"static string.Format<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)")]
	public extern static string _850c49e163cd3ed0<TArg0, TArg1, TArg2>(Intl.NumberFormat? provider, object format, object arg0, object arg1, object arg2);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params object[])")]
	public extern static string _1183035ecb38f2a4(Intl.NumberFormat? provider, object format,  object args);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[Jazor(Op.Discard ,"static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)")]
	public extern static string _e4458a04839fcdc5(Intl.NumberFormat? provider, object format,  object args);

	/// <summary>
	/// C#: str.Insert(startIndex, value)
	/// JS: str.slice(0, startIndex) + value + str.slice(startIndex)
	/// </summary>
	[Jazor(Op.Inline, "string.Insert(int, string)", "(__arg1.slice(0, __arg2) + __arg3 + __arg1.slice(__arg2))")]
	public extern static string _91223088dad76801(string instance, Number startIndex, string value);

	///<summary>Concatenates an array of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join(char, params string[])")]
	public static string _14ec7ebbb72b7d13(string separator, Array<string?> value)
		=> ConcatStrings(value, separator, "value");

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join(char, params System.ReadOnlySpan<string>)")]
	public static string _9f939553178c2ca6(string separator, Array<string?> value)
		=> ConcatStrings(value, separator, "value");

	///<summary>Concatenates all the elements of a string array, using the specified separator between each element.</summary>
	[Jazor(Op.Import ,"static string.Join(string, params string[])")]
	public static string _f269cd27a4bbd549(string? separator, Array<string?> value)
		=> ConcatStrings(value, separator ?? "", "value");

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join(string, params System.ReadOnlySpan<string>)")]
	public static string _224682d778b9facf(string? separator, Array<string?> value)
		=> ConcatStrings(value, separator ?? "", "value");

	///<summary>Concatenates an array of strings, using the specified separator between each member, starting with the element in <paramref name="value" /> located at the <paramref name="startIndex" /> position, and concatenating up to <paramref name="count" /> elements.</summary>
	[Jazor(Op.Import ,"static string.Join(char, string[], int, int)")]
	public static string _f461a3c632706317(string separator, Array<string?> value, Number startIndex, Number count)
		=> JoinRange(separator, value, startIndex, count);

	///<summary>Concatenates the specified elements of a string array, using the specified separator between each element.</summary>
	[Jazor(Op.Import ,"static string.Join(string, string[], int, int)")]
	public static string _f1ad756b7baec84b(string? separator, Array<string?> value, Number startIndex, Number count)
		=> JoinRange(separator ?? "", value, startIndex, count);

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join(string, System.Collections.Generic.IEnumerable<string>)")]
	public static string _d8814705c8078096(string? separator, IEnumerable<string?> values)
		=> ConcatStrings(values, separator ?? "", "values");

	///<summary>Concatenates the string representations of an array of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join(char, params object[])")]
	public static string _5ac0762c6816a423(string separator, Array<object?> values)
		=> ConcatValues(values, separator, "values");

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Import, "static string.Join(char, params System.ReadOnlySpan<object>)")]
	public static string _477a1f45d63f93c2(string separator, Array<object?> values)
		=> ConcatValues(values, separator, "values");

	///<summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
	[Jazor(Op.Import ,"static string.Join(string, params object[])")]
	public static string _c69ae51b8f3b72f0(string? separator, Array<object?> values)
		=> ConcatValues(values, separator ?? "", "values");

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Import, "static string.Join(string, params System.ReadOnlySpan<object>)")]
	public static string _f8903c473c9e5f05(string? separator, Array<object?> values)
		=> ConcatValues(values, separator ?? "", "values");

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)")]
	public static string _1c599eccbbc8f2b8<T>(string separator, IEnumerable<T> values)
		=> ConcatValues(values, separator, "values");

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[Jazor(Op.Import ,"static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)")]
	public static string _c78854b22e947a4f<T>(string? separator, IEnumerable<T> values)
		=> ConcatValues(values, separator ?? "", "values");

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
	[Jazor(Op.Import ,"string.PadLeft(int)")]
	public static string _26620c4bafb4f435(string instance, Number totalWidth)
	{
		EnsureNonNegativeWholeNumber(totalWidth, nameof(totalWidth));
		return instance.PadStart(totalWidth, " ");
	}

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
	[Jazor(Op.Import ,"string.PadLeft(int, char)")]
	public static string _7894e0294f780eb5(string instance, Number totalWidth, string paddingChar)
	{
		EnsureNonNegativeWholeNumber(totalWidth, nameof(totalWidth));
		return instance.PadStart(totalWidth, paddingChar);
	}

	///<summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
	[Jazor(Op.Import ,"string.PadRight(int)")]
	public static string _0e8f0a28fc1de8c2(string instance, Number totalWidth)
	{
		EnsureNonNegativeWholeNumber(totalWidth, nameof(totalWidth));
		return instance.PadEnd(totalWidth, " ");
	}

	///<summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
	[Jazor(Op.Import ,"string.PadRight(int, char)")]
	public static string _685227781124d327(string instance, Number totalWidth, string paddingChar)
	{
		EnsureNonNegativeWholeNumber(totalWidth, nameof(totalWidth));
		return instance.PadEnd(totalWidth, paddingChar);
	}

	/// <summary>
	/// C#: str.Remove(startIndex, count)
	/// JS: str.slice(0, startIndex) + str.slice(startIndex + count)
	/// </summary>
	[Jazor(Op.Inline, "string.Remove(int, int)", "(__arg1.slice(0, __arg2) + __arg1.slice(__arg2 + __arg3))")]
	public extern static string _ac075983805231a6(string instance, Number startIndex, Number count);

	/// <summary>
	/// C#: str.Remove(startIndex)
	/// JS: str.slice(0, startIndex)
	/// </summary>
	[Jazor(Op.Inline, "string.Remove(int)", "__arg1.slice(0, __arg2)")]
	public extern static string _d258363cef56cdfb(string instance, Number startIndex);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided culture and case sensitivity.</summary>
	[Jazor(Op.Discard ,"string.Replace(string, string, bool, System.Globalization.CultureInfo)")]
	public extern static string _80ebf2c83f8072e2(string instance, string oldValue, string? newValue, bool ignoreCase, String? culture);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.</summary>
	[Jazor(Op.Import ,"string.Replace(string, string, System.StringComparison)")]
	public static string _8a7510653022a974(string instance, string oldValue, string? newValue, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? ReplaceAllIgnoreCase(instance, oldValue, newValue ?? "")
			: instance.ReplaceAll(oldValue, newValue ?? "");

	///<summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
	[Jazor(Op.Import ,"string.Replace(char, char)")]
	public static string _7d7cb13bbbbb83c8(string instance, Number oldChar, Number newChar)
		=> instance.ReplaceAll(oldChar.ToString(), newChar.ToString());

	/// <summary>
	/// C#: str.Replace(oldValue, newValue)
	/// JS: str.replaceAll(oldValue, newValue)
	/// Note: Use replaceAll to replace all occurrences
	/// </summary>
	[Jazor(Op.Alias, "string.Replace(string, string)", "replaceAll")]
	public extern static string _78a0e353c29afbc9(string instance, string oldValue, string? newValue);

	///<summary>Replaces all newline sequences in the current string with <see cref="P:System.Environment.NewLine" />.</summary>
	[Jazor(Op.Import, "string.ReplaceLineEndings()")]
	public static string _3720e4de26fa4c1b(string instance)
		// Jazor runtime artifacts execute under Deno.host, where Environment.NewLine is LF.
		=> ReplaceLineEndingsCore(instance, "\n");

	///<summary>Replaces all newline sequences in the current string with <paramref name="replacementText" />.</summary>
	[Jazor(Op.Import ,"string.ReplaceLineEndings(string)")]
	public static string _35041c0250b36108(string instance, string replacementText)
		=> ReplaceLineEndingsCore(instance, replacementText);

	///<summary>Splits a string into substrings based on a specified delimiting character and, optionally, options.</summary>
	[Jazor(Op.Import ,"string.Split(char, System.StringSplitOptions)")]
	public static string[] _d8080c573d45b4b4(string instance, Number separator, object options)
		=> ApplySplitOptions(instance.Split(separator.ToString(), NumberFn(instance.Length + 1)), options);

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting character and, optionally, options.        Splits a string into a maximum number of substrings based on the provided character separator, optionally omitting empty substrings from the result.</summary>
	[Jazor(Op.Import ,"string.Split(char, int, System.StringSplitOptions)")]
	public static string[] _aaa73a4811837ec7(string instance, Number separator, Number count, object options)
	{
		if (count <= 0)
			return [];

		if (count == 1)
			return ApplySplitOptions([instance], options);

		var trimEntries = false;
		var removeEmptyEntries = false;
		if (options is Number splitOptions)
		{
			trimEntries = (splitOptions & 2) != 0;
			removeEmptyEntries = (splitOptions & 1) != 0;
		}

		var token = separator.ToString();
		var result = new Array<string>();
		var start = 0;
		while (result.Length < count - 1)
		{
			var index = instance.IndexOf(token, start);
			if (index < 0)
				break;

			var part = instance.Substring(start, index - start);
			part = trimEntries ? part.Trim() : part;
			if (!removeEmptyEntries || part.Length != 0)
				result.Push(part);

			start = index + token.Length;
		}

		var tail = instance.Substring(start);
		tail = trimEntries ? tail.Trim() : tail;
		if (!removeEmptyEntries || tail.Length != 0)
			result.Push(tail);

		return result;
	}

	/// <summary>
	/// C#: str.Split(separatorChars)
	/// JS: str.split(RegExp("[...]"))
	/// Note: 多字符分隔在 JS 中不能直接传数组给 split；这里统一构造字符类正则。
	/// </summary>
	[Jazor(Op.Import, "string.Split(params char[])")]
	public static string[] _62c8810ea13dba45(string instance, object? separator)
	{
		if (separator is null)
			return instance.Split(RegExp(@"\s+"));

		if (separator is string singleSeparator)
			return instance.Split(RegExp(BuildSplitCharClassPattern(singleSeparator)));

		if (separator is Array<string> separators)
			return instance.Split(RegExp(BuildSplitCharClassPattern(separators)));

		return instance.Split(RegExp(@"\s+"));
	}

	private static string BuildSplitCharClassPattern(string separator)
	{
		if (separator.Length == 0)
			return @"\s+";

		var pattern = "[";
		for (var i = 0; i < separator.Length; i++)
			pattern += EscapeRegexCharClassChar(separator.Substring(i, 1));

		return pattern + "]";
	}

	private static string BuildSplitCharClassPattern(Array<string> separators)
	{
		if (separators.Length == 0)
			return @"\s+";

		var pattern = "[";
		var hasSeparator = false;
		for (var i = 0; i < separators.Length; i++)
		{
			var separator = (string?)separators[i];
			if (separator is null || separator.Length == 0)
				continue;

			hasSeparator = true;
			for (var j = 0; j < separator.Length; j++)
				pattern += EscapeRegexCharClassChar(separator.Substring(j, 1));
		}

		return hasSeparator ? pattern + "]" : @"\s+";
	}

	private static string EscapeRegexCharClassChar(string ch) => ch switch
	{
		"\\" => "\\\\",
		"]" => "\\]",
		"^" => "\\^",
		"-" => "\\-",
		_ => ch
	};

	private static string[] ApplySplitOptions(string[] parts, object options)
	{
		var trimEntries = false;
		var removeEmptyEntries = false;
		if (options is Number splitOptions)
		{
			trimEntries = (splitOptions & 2) != 0;
			removeEmptyEntries = (splitOptions & 1) != 0;
		}

		var result = new Array<string>();
		foreach (var part in parts)
		{
			var current = trimEntries ? part.Trim() : part;
			if (removeEmptyEntries && current.Length == 0)
				continue;

			result.Push(current);
		}

		return result;
	}

	private static string[] SplitByCharSetWithLimitAndOptions(string instance, object separator, Number count, object options)
	{
		if (count <= 0)
			return [];

		var trimEntries = false;
		var removeEmptyEntries = false;
		if (options is Number splitOptions)
		{
			trimEntries = (splitOptions & 2) != 0;
			removeEmptyEntries = (splitOptions & 1) != 0;
		}

		if (count == 1)
			return ApplySplitOptions([instance], options);

		var any = NormalizeCharSet(separator);
		var result = new Array<string>();
		var start = 0;
		for (var i = 0; i < instance.Length && result.Length < count - 1; i++)
		{
			if (!any.Contains(instance[i].ToString()))
				continue;

			var part = instance.Substring(start, i - start);
			part = trimEntries ? part.Trim() : part;
			if (!removeEmptyEntries || part.Length != 0)
				result.Push(part);

			start = i + 1;
		}

		var tail = instance.Substring(start);
		tail = trimEntries ? tail.Trim() : tail;
		if (!removeEmptyEntries || tail.Length != 0)
			result.Push(tail);

		return result;
	}

	private static string[] SplitByStringsWithLimitAndOptions(string instance, object separator, Number count, object options)
	{
		if (count <= 0)
			return [];

		var trimEntries = false;
		var removeEmptyEntries = false;
		if (options is Number splitOptions)
		{
			trimEntries = (splitOptions & 2) != 0;
			removeEmptyEntries = (splitOptions & 1) != 0;
		}

		if (count == 1)
			return ApplySplitOptions([instance], options);

		var separators = NormalizeStringSeparators(separator);
		var result = new Array<string>();
		var start = 0;
		while (result.Length < count - 1)
		{
			var bestIndex = -1;
			string? bestSeparator = null;
			for (var i = 0; i < separators.Length; i++)
			{
				var item = (string?)separators[i];
				if (item is null)
					continue;

				var index = instance.IndexOf(item, start);
				if (index < 0)
					continue;

				if (bestIndex < 0 || index < bestIndex)
				{
					bestIndex = index;
					bestSeparator = item;
				}
			}

			if (bestIndex < 0 || bestSeparator is null)
				break;

			var part = instance.Substring(start, bestIndex - start);
			part = trimEntries ? part.Trim() : part;
			if (!removeEmptyEntries || part.Length != 0)
				result.Push(part);

			start = bestIndex + bestSeparator.Length;
		}

		var tail = instance.Substring(start);
		tail = trimEntries ? tail.Trim() : tail;
		if (!removeEmptyEntries || tail.Length != 0)
			result.Push(tail);

		return result;
	}

	private static string ReplaceAllIgnoreCase(string instance, string oldValue, string newValue)
	{
		if (oldValue.Length == 0)
			return instance;

		var source = instance.ToLower();
		var target = oldValue.ToLower();
		var result = "";
		var start = 0;
		while (true)
		{
			var index = source.IndexOf(target, start);
			if (index < 0)
				break;

			result += instance.Substring(start, index - start);
			result += newValue;
			start = index + oldValue.Length;
		}

		return start == 0 ? instance : result + instance.Substring(start);
	}

	private static Array<string> NormalizeStringSeparators(object separator)
	{
		var result = new Array<string>();
		switch (separator)
		{
			case string single when single.Length != 0:
				result.Push(single);
				break;
			case Array<string> many:
				for (var i = 0; i < many.Length; i++)
				{
					var item = (string?)many[i];
					if (item is not null && item.Length != 0)
						result.Push(item);
				}

				break;
		}

		return result;
	}

	///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
	[Jazor(Op.Import ,"string.Split(params System.ReadOnlySpan<char>)")]
	public static string[] _5417a93b3075813a(string instance, object? separator)
		=> _62c8810ea13dba45(instance, separator);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters.</summary>
	[Jazor(Op.Import ,"string.Split(char[], int)")]
	public static string[] _d03d120228c0c4ed(string instance, object separator, Number count)
		=> SplitByCharSetWithLimitAndOptions(instance, separator, count, 0);

	///<summary>Splits a string into substrings based on specified delimiting characters and options.</summary>
	[Jazor(Op.Import ,"string.Split(char[], System.StringSplitOptions)")]
	public static string[] _25c1f15b0ed2cb6e(string instance, object separator, object options)
		=> ApplySplitOptions(_62c8810ea13dba45(instance, separator), options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters and, optionally, options.</summary>
	[Jazor(Op.Import ,"string.Split(char[], int, System.StringSplitOptions)")]
	public static string[] _c8e5ceed33c6c638(string instance, object separator, Number count, object options)
		=> SplitByCharSetWithLimitAndOptions(instance, separator, count, options);

	///<summary>Splits a string into substrings that are based on the provided string separator.</summary>
	[Jazor(Op.Import ,"string.Split(string, System.StringSplitOptions)")]
	public static string[] _189761f781df8770(string instance, string? separator, object options)
	{
		if (separator == null)
			return ApplySplitOptions([instance], options);

		return ApplySplitOptions(instance.Split(separator, NumberFn(instance.Length + 1)), options);
	}

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.</summary>
	[Jazor(Op.Import ,"string.Split(string, int, System.StringSplitOptions)")]
	public static string[] _96eb0a23afa7fdfb(string instance, string? separator, Number count, object options)
	{
		if (count <= 0)
			return [];

		if (count == 1)
			return ApplySplitOptions([instance], options);

		if (string.IsNullOrEmpty(separator))
			return ApplySplitOptions([instance], options);

		var trimEntries = false;
		var removeEmptyEntries = false;
		if (options is Number splitOptions)
		{
			trimEntries = (splitOptions & 2) != 0;
			removeEmptyEntries = (splitOptions & 1) != 0;
		}

		var result = new Array<string>();
		var start = 0;
		while (result.Length < count - 1)
		{
			var index = instance.IndexOf(separator, start);
			if (index < 0)
				break;

			var part = instance.Substring(start, index - start);
			part = trimEntries ? part.Trim() : part;
			if (!removeEmptyEntries || part.Length != 0)
				result.Push(part);

			start = index + separator.Length;
		}

		var tail = instance.Substring(start);
		tail = trimEntries ? tail.Trim() : tail;
		if (!removeEmptyEntries || tail.Length != 0)
			result.Push(tail);

		return result;
	}

	///<summary>Splits a string into substrings based on a specified delimiting string and, optionally, options.</summary>
	[Jazor(Op.Import ,"string.Split(string[], System.StringSplitOptions)")]
	public static string[] _fff99c96206a241e(string instance, object separator, object options)
		=> SplitByStringsWithLimitAndOptions(instance, separator, instance.Length + 1, options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting strings and, optionally, options.</summary>
	[Jazor(Op.Import ,"string.Split(string[], int, System.StringSplitOptions)")]
	public static string[] _f3c7edcc7cc89a4a(string instance, object separator, Number count, object options)
		=> SplitByStringsWithLimitAndOptions(instance, separator, count, options);

	/// <summary>
	/// C#: str.Substring(startIndex)
	/// JS: str.substring(startIndex)
	/// </summary>
	[Jazor(Op.Alias, "string.Substring(int)", "substring")]
	public extern static string _6b947e3ae92ce851(string instance, Number startIndex);

	/// <summary>
	/// C#: str.Substring(startIndex, length)
	/// JS: str.substring(startIndex, startIndex + length)
	/// Note: C# Substring uses length, JS substring uses end index
	/// </summary>
	[Jazor(Op.Inline, "string.Substring(int, int)", "__arg1.substring(__arg2, __arg2 + __arg3)")]
	public extern static string _ac659b5819c0360c(string instance, Number startIndex, Number length);

	/// <summary>
	/// C#: str.ToLower()
	/// JS: str.toLowerCase()
	/// </summary>
	[Jazor(Op.Alias, "string.ToLower()", "toLowerCase")]
	public extern static string _482205d85705de41(string instance);

	///<summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
	[Jazor(Op.Discard ,"string.ToLower(System.Globalization.CultureInfo)")]
	public extern static string _8e06da9945efff04(string instance, String? culture);

	/// <summary>
	/// C#: str.ToLowerInvariant()
	/// JS: str.toLowerCase()
	/// </summary>
	[Jazor(Op.Alias, "string.ToLowerInvariant()", "toLowerCase")]
	public extern static string _3ff043d0307f4917(string instance);

	/// <summary>
	/// C#: str.ToUpper()
	/// JS: str.toUpperCase()
	/// </summary>
	[Jazor(Op.Alias, "string.ToUpper()", "toUpperCase")]
	public extern static string _4b84099d877364bd(string instance);

	///<summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
	[Jazor(Op.Discard ,"string.ToUpper(System.Globalization.CultureInfo)")]
	public extern static string _9369d4b370002404(string instance, String? culture);

	/// <summary>
	/// C#: str.ToUpperInvariant()
	/// JS: str.toUpperCase()
	/// </summary>
	[Jazor(Op.Alias, "string.ToUpperInvariant()", "toUpperCase")]
	public extern static string _3dc9c0782170eb46(string instance);

	/// <summary>
	/// C#: str.Trim()
	/// JS: str.trim()
	/// </summary>
	[Jazor(Op.Alias, "string.Trim()", "trim")]
	public extern static string _eb98ee79e16b7ad4(string instance);

	/// <summary>
	/// C#: str.Trim(trimChar)
	/// JS: 移除首尾连续的指定字符
	/// </summary>
	[Jazor(Op.Import ,"string.Trim(char)")]
	public static string _5d7e005b9dcb67de(string instance, Number trimChar)
	{
		var token = trimChar.ToString() ?? "";
		if (token.Length == 0)
			return instance;

		var start = 0;
		var end = instance.Length - 1;
		while (start <= end && instance[start].ToString() == token)
			start++;

		while (end >= start && instance[end].ToString() == token)
			end--;

		return start > end ? string.Empty : instance.Substring(start, end - start + 1);
	}

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Import ,"string.Trim(params char[])")]
	public static string _c6c444b4e71e14f7(string instance, Array<string>? trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
			return instance.Trim();
		return TrimCharacterSet(instance, NormalizeCharSet(trimChars), trimStart: true, trimEnd: true);
	}

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Import, "string.Trim(params System.ReadOnlySpan<char>)")]
	public static string _0e8e4169883e5222(string instance, RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimReadOnlyCharacterSpan(instance, trimChars, trimStart: true, trimEnd: true);

	/// <summary>
	/// C#: str.TrimStart()
	/// JS: str.trimStart()
	/// </summary>
	[Jazor(Op.Alias, "string.TrimStart()", "trimStart")]
	public extern static string _1ca7f6e7edd1e070(string instance);

	/// <summary>
	/// C#: str.TrimStart(trimChar)
	/// JS: 移除开头连续的指定字符
	/// </summary>
	[Jazor(Op.Import ,"string.TrimStart(char)")]
	public static string _561fe737e62cf332(string instance, Number trimChar)
	{
		var token = trimChar.ToString() ?? "";
		if (token.Length == 0)
			return instance;

		var start = 0;
		while (start < instance.Length && instance[start].ToString() == token)
			start++;

		return start == 0 ? instance : instance.Substring(start);
	}

	///<summary>Removes all the leading occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Import ,"string.TrimStart(params char[])")]
	public static string _98731360726c6976(string instance, Array<string>? trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
			return instance.TrimStart();
		return TrimCharacterSet(instance, NormalizeCharSet(trimChars), trimStart: true, trimEnd: false);
	}

	///<summary>Removes all the leading occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Import, "string.TrimStart(params System.ReadOnlySpan<char>)")]
	public static string _f0473806a2e03bb6(string instance, RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimReadOnlyCharacterSpan(instance, trimChars, trimStart: true, trimEnd: false);

	/// <summary>
	/// C#: str.TrimEnd()
	/// JS: str.trimEnd()
	/// </summary>
	[Jazor(Op.Alias, "string.TrimEnd()", "trimEnd")]
	public extern static string _760bdb666072200b(string instance);

	/// <summary>
	/// C#: str.TrimEnd(trimChar)
	/// JS: 移除结尾连续的指定字符
	/// </summary>
	[Jazor(Op.Import ,"string.TrimEnd(char)")]
	public static string _eb362a090d734099(string instance, Number trimChar)
	{
		var token = trimChar.ToString() ?? "";
		if (token.Length == 0)
			return instance;

		var end = instance.Length - 1;
		while (end >= 0 && instance[end].ToString() == token)
			end--;

		return end == instance.Length - 1 ? instance : end < 0 ? string.Empty : instance.Substring(0, end + 1);
	}

	///<summary>Removes all the trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Import ,"string.TrimEnd(params char[])")]
	public static string _a62862c1fbaa21c3(string instance, Array<string>? trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
			return instance.TrimEnd();
		return TrimCharacterSet(instance, NormalizeCharSet(trimChars), trimStart: false, trimEnd: true);
	}

	///<summary>Removes all the trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Import, "string.TrimEnd(params System.ReadOnlySpan<char>)")]
	public static string _4f8d256566de4b17(string instance, RuntimeModule.JReadOnlyCharSpan trimChars)
		=> TrimReadOnlyCharacterSpan(instance, trimChars, trimStart: false, trimEnd: true);

	/// <summary>
	/// C#: str.Contains(value)
	/// JS: str.includes(value)
	/// </summary>
	[Jazor(Op.Alias, "string.Contains(string)", "includes")]
	public extern static bool _c42ed9bafadfb16c(string instance, string value);

	///<summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
	[Jazor(Op.Import ,"string.Contains(string, System.StringComparison)")]
	public static bool _d52d7114d5c1b839(string instance, string value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().Includes(value.ToLower())
			: instance.Includes(value);

	/// <summary>
	/// C#: str.Contains(value)
	/// JS: str.includes(value)
	/// </summary>
	[Jazor(Op.Alias, "string.Contains(char)", "includes")]
	public extern static bool _5de05262ccc56b2e(string instance, Number value);

	///<summary>Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.</summary>
	[Jazor(Op.Import ,"string.Contains(char, System.StringComparison)")]
	public static bool _16d4b2b4de019fb2(string instance, Number value, object comparisonType)
	{
		var token = value.ToString();
		return IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().Includes(token.ToLower())
			: instance.Includes(token);
	}

	/// <summary>
	/// C#: str.IndexOf(value)
	/// JS: str.indexOf(value)
	/// </summary>
	[Jazor(Op.Alias, "string.IndexOf(char)", "indexOf")]
	public extern static Number _9c8b4ffa28964fba(string instance, Number value);

	/// <summary>
	/// C#: str.IndexOf(value, startIndex)
	/// JS: str.indexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Alias, "string.IndexOf(char, int)", "indexOf")]
	public extern static Number _c98394955f62f130(string instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. A parameter specifies the type of search to use for the specified character.</summary>
	[Jazor(Op.Import ,"string.IndexOf(char, System.StringComparison)")]
	public static Number _5331447e2c855a66(string instance, Number value, object comparisonType)
	{
		var token = value.ToString();
		return IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().IndexOf(token.ToLower())
			: instance.IndexOf(token);
	}

	///<summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.IndexOf(char, int, int)")]
	public static Number _d2873e605fbed764(string instance, Number value, Number startIndex, Number count)
	{
		var target = value.ToString();
		var end = startIndex + count;
		for (var i = startIndex; i < end && i < instance.Length; i++)
		{
			if (instance[i].ToString() == target)
				return i;
		}

		return -1;
	}

	/// <summary>
	/// C#: str.IndexOfAny(anyOf)
	/// JS: 返回任一字符首次出现的位置
	/// </summary>
	[Jazor(Op.Import ,"string.IndexOfAny(char[])")]
	public static Number _69b749a1c6cbae78(string instance, object anyOf)
	{
		var any = NormalizeCharSet(anyOf);
		for (var i = 0; i < instance.Length; i++)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
	[Jazor(Op.Import ,"string.IndexOfAny(char[], int)")]
	public static Number _63633a5f3b85c5a9(string instance, object anyOf, Number startIndex)
	{
		var any = NormalizeCharSet(anyOf);
		for (var i = startIndex; i < instance.Length; i++)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.IndexOfAny(char[], int, int)")]
	public static Number _cb863079aae72451(string instance, object anyOf, Number startIndex, Number count)
	{
		var any = NormalizeCharSet(anyOf);
		var end = startIndex + count;
		for (var i = startIndex; i < end && i < instance.Length; i++)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	/// <summary>
	/// C#: str.IndexOf(value)
	/// JS: str.indexOf(value)
	/// </summary>
	[Jazor(Op.Alias, "string.IndexOf(string)", "indexOf")]
	public extern static Number _6fd03b0f0c2de338(string instance, string value);

	/// <summary>
	/// C#: str.IndexOf(value, startIndex)
	/// JS: str.indexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Alias, "string.IndexOf(string, int)", "indexOf")]
	public extern static Number _8c391718b5fbe536(string instance, string value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.IndexOf(string, int, int)")]
	public static Number _ff549d811898fb56(string instance, string value, Number startIndex, Number count)
	{
		var end = startIndex + count - value.Length;
		for (var i = startIndex; i <= end && i + value.Length <= instance.Length; i++)
		{
			if (instance.Substring(i, value.Length) == value)
				return i;
		}

		return -1;
	}

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[Jazor(Op.Import ,"string.IndexOf(string, System.StringComparison)")]
	public static Number _3ae4900da2b07b27(string instance, string value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().IndexOf(value.ToLower())
			: instance.IndexOf(value);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
	[Jazor(Op.Import ,"string.IndexOf(string, int, System.StringComparison)")]
	public static Number _2fabe2b831abe71e(string instance, string value, Number startIndex, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().IndexOf(value.ToLower(), startIndex)
			: instance.IndexOf(value, startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
	[Jazor(Op.Import ,"string.IndexOf(string, int, int, System.StringComparison)")]
	public static Number _ab22561fc42166db(string instance, string value, Number startIndex, Number count, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? _ff549d811898fb56(instance.ToLower(), value.ToLower(), startIndex, count)
			: _ff549d811898fb56(instance, value, startIndex, count);

	/// <summary>
	/// C#: str.LastIndexOf(value)
	/// JS: str.lastIndexOf(value)
	/// </summary>
	[Jazor(Op.Alias, "string.LastIndexOf(char)", "lastIndexOf")]
	public extern static Number _da9a8971cb787f7f(string instance, Number value);

	/// <summary>
	/// C#: str.LastIndexOf(value, startIndex)
	/// JS: str.lastIndexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Alias, "string.LastIndexOf(char, int)", "lastIndexOf")]
	public extern static Number _b21118cfc4c55581(string instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.LastIndexOf(char, int, int)")]
	public static Number _dbdd57f8d259ce66(string instance, Number value, Number startIndex, Number count)
	{
		var target = value.ToString();
		var end = startIndex >= instance.Length ? NumberFn(instance.Length - 1) : startIndex;
		var begin = end - count + 1;
		if (begin < 0)
			begin = 0;

		for (var i = end; i >= begin; i--)
		{
			if (instance[i].ToString() == target)
				return i;
		}

		return -1;
	}

	/// <summary>
	/// C#: str.LastIndexOfAny(anyOf)
	/// JS: 返回任一字符最后一次出现的位置
	/// </summary>
	[Jazor(Op.Import ,"string.LastIndexOfAny(char[])")]
	public static Number _c0212f4213a99019(string instance, object anyOf)
	{
		var any = NormalizeCharSet(anyOf);
		for (var i = instance.Length - 1; i >= 0; i--)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	private static HashSet<string> NormalizeCharSet(object anyOf)
	{
		var set = new HashSet<string>();

		switch (anyOf)
		{
			case string single:
				for (var i = 0; i < single.Length; i++)
					set.Add(single[i].ToString());
				break;
			case Array<string> many:
				for (var i = 0; i < many.Length; i++)
				{
					var item = (string?)many[i];
					if (string.IsNullOrEmpty(item))
						continue;

					for (var j = 0; j < item.Length; j++)
						set.Add(item[j].ToString());
				}
				break;
		}

		return set;
	}

	private static string TrimCharacterSet(
		string instance,
		HashSet<string> characters,
		bool trimStart,
		bool trimEnd)
	{
		var start = 0;
		var end = instance.Length - 1;
		if (trimStart)
		{
			while (start <= end && characters.Contains(instance[start].ToString()))
				start++;
		}
		if (trimEnd)
		{
			while (end >= start && characters.Contains(instance[end].ToString()))
				end--;
		}

		if (start == 0 && end == instance.Length - 1)
			return instance;
		return start > end ? string.Empty : instance.Substring(start, end - start + 1);
	}

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[Jazor(Op.Import ,"string.LastIndexOfAny(char[], int)")]
	public static Number _c401e64318e768c4(string instance, object anyOf, Number startIndex)
	{
		var any = NormalizeCharSet(anyOf);
		var index = startIndex >= instance.Length ? NumberFn(instance.Length - 1) : startIndex;
		for (var i = index; i >= 0; i--)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.LastIndexOfAny(char[], int, int)")]
	public static Number _3c17fcef5615e7a3(string instance, object anyOf, Number startIndex, Number count)
	{
		var any = NormalizeCharSet(anyOf);
		var end = startIndex >= instance.Length ? NumberFn(instance.Length - 1) : startIndex;
		var begin = end - count + 1;
		if (begin < 0)
			begin = 0;

		for (var i = end; i >= begin; i--)
		{
			var current = instance[i].ToString();
			if (any.Contains(current))
				return i;
		}

		return -1;
	}

	/// <summary>
	/// C#: str.LastIndexOf(value)
	/// JS: str.lastIndexOf(value)
	/// </summary>
	[Jazor(Op.Alias, "string.LastIndexOf(string)", "lastIndexOf")]
	public extern static Number _ed4ccee87d9df9fc(string instance, string value);

	/// <summary>
	/// C#: str.LastIndexOf(value, startIndex)
	/// JS: str.lastIndexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Alias, "string.LastIndexOf(string, int)", "lastIndexOf")]
	public extern static Number _404d5ed27b7e190a(string instance, string value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Import ,"string.LastIndexOf(string, int, int)")]
	public static Number _c4ee024d06ee238c(string instance, string value, Number startIndex, Number count)
	{
		var end = startIndex >= instance.Length ? NumberFn(instance.Length - 1) : startIndex;
		var begin = end - count + 1;
		if (begin < 0)
			begin = 0;

		var maxStart = end - value.Length + 1;
		for (var i = maxStart; i >= begin; i--)
		{
			if (i >= 0 && i + value.Length <= instance.Length && instance.Substring(i, value.Length) == value)
				return i;
		}

		return -1;
	}

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[Jazor(Op.Import ,"string.LastIndexOf(string, System.StringComparison)")]
	public static Number _78449c135e18c4bc(string instance, string value, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().LastIndexOf(value.ToLower())
			: instance.LastIndexOf(value);

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[Jazor(Op.Import ,"string.LastIndexOf(string, int, System.StringComparison)")]
	public static Number _359dbce44ce4a4da(string instance, string value, Number startIndex, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? instance.ToLower().LastIndexOf(value.ToLower(), startIndex)
			: instance.LastIndexOf(value, startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[Jazor(Op.Import ,"string.LastIndexOf(string, int, int, System.StringComparison)")]
	public static Number _c911a06f021bd138(string instance, string value, Number startIndex, Number count, object comparisonType)
		=> IsOrdinalIgnoreCase(comparisonType)
			? _c4ee024d06ee238c(instance.ToLower(), value.ToLower(), startIndex, count)
			: _c4ee024d06ee238c(instance, value, startIndex, count);
}
