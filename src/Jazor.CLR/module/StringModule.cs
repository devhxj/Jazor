namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "string","System/StringModule.js")]
public static class StringModule
{
	///<summary>Represents the empty string. This field is read-only.</summary>
	[Jazor(Op.Inline, "static readonly string.Empty", "\\\"\\\"")]
	public extern static string _b16f79dc7b155be3();

	///<summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"static string.Intern(string)")]
	public extern static string _1234444e218b96c3(string str);

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
	[Jazor(Op.Discard ,"static string.Compare(string, string, System.StringComparison)")]
	public extern static Number _9d940114ace1198f(string? strA, string? strB, object comparisonType);

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
	[Jazor(Op.Discard ,"static string.Compare(string, int, string, int, int, System.StringComparison)")]
	public extern static Number _d78fb9d76fca75e4(string? strA, Number indexA, string? strB, Number indexB, Number length, object comparisonType);

	///<summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
	[Jazor(Op.Discard ,"static string.CompareOrdinal(string, string)")]
	public extern static Number _a55d307de6e31c7b(string? strA, string? strB);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring.</summary>
	[Jazor(Op.Discard ,"static string.CompareOrdinal(string, int, string, int, int)")]
	public extern static Number _dc789454b6ef6bcb(string? strA, Number indexA, string? strB, Number indexB, Number length);

	///<summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	[Jazor(Op.Discard ,"string.CompareTo(object)")]
	public extern static Number _629b0613344d82e7(string instance, object? value);

	///<summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified string.</summary>
	[Jazor(Op.Discard ,"string.CompareTo(string)")]
	public extern static Number _380e7c7649d703f0(string instance, string? strB);

	/// <summary>
	/// C#: str.EndsWith(value)
	/// JS: str.endsWith(value)
	/// </summary>
	[Jazor(Op.Replace, "string.EndsWith(string)", "endsWith")]
	public extern static bool _33de316681320ec7(string instance, string value);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[Jazor(Op.Discard ,"string.EndsWith(string, System.StringComparison)")]
	public extern static bool _946b7129a48c8114(string instance, string value, object comparisonType);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
	[Jazor(Op.Discard ,"string.EndsWith(string, bool, System.Globalization.CultureInfo)")]
	public extern static bool _679207cac049d3c6(string instance, string value, bool ignoreCase, String? culture);

	///<summary>Determines whether the end of this string instance matches the specified character.</summary>
	[Jazor(Op.Discard ,"string.EndsWith(char)")]
	public extern static bool _7619ce4eda48c8e8(string instance, Number value);

	///<summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
	[Jazor(Op.Discard ,"override string.Equals(object)")]
	public extern static bool _def18c2802a57249(string instance, object? obj);

	///<summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
	[Jazor(Op.Discard ,"string.Equals(string)")]
	public extern static bool _6ee9bc86e4384225(string instance, string? value);

	///<summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[Jazor(Op.Discard ,"string.Equals(string, System.StringComparison)")]
	public extern static bool _f8e1e01e8c17e8bb(string instance, string? value, object comparisonType);

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
	[Jazor(Op.Discard ,"static string.Equals(string, string)")]
	public extern static bool _e6b1648151c863d5(string? a, string? b);

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[Jazor(Op.Discard ,"static string.Equals(string, string, System.StringComparison)")]
	public extern static bool _b7c36408f0f172e9(string? a, string? b, object comparisonType);

	///<summary>Determines whether two specified strings have the same value.</summary>
	[Jazor(Op.Allowed ,"static string.operator ==(string, string)")]
	public extern static bool _ee27dec45b308755(string? a, string? b);

	///<summary>Determines whether two specified strings have different values.</summary>
	[Jazor(Op.Allowed ,"static string.operator !=(string, string)")]
	public extern static bool _1573803c425863d3(string? a, string? b);

	///<summary>Returns the hash code for this string.</summary>
	[Jazor(Op.Discard ,"override string.GetHashCode()")]
	public extern static Number _bccdd3f386a6fbbc(string instance);

	///<summary>Returns the hash code for this string using the specified rules.</summary>
	[Jazor(Op.Discard ,"string.GetHashCode(System.StringComparison)")]
	public extern static Number _04edfc3090710ca7(string instance, object comparisonType);

	///<summary>Returns the hash code for the provided read-only character span.</summary>
	[Jazor(Op.Discard ,"static string.GetHashCode(System.ReadOnlySpan<char>)")]
	public extern static Number _4598a18be32f839d(Uint32Array value);

	///<summary>Returns the hash code for the provided read-only character span using the specified rules.</summary>
	[Jazor(Op.Discard ,"static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)")]
	public extern static Number _d123047f69d911f5(Uint32Array value, object comparisonType);

	/// <summary>
	/// C#: str.StartsWith(value)
	/// JS: str.startsWith(value)
	/// </summary>
	[Jazor(Op.Replace, "string.StartsWith(string)", "startsWith")]
	public extern static bool _1cda198f8257d023(string instance, string value);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[Jazor(Op.Discard ,"string.StartsWith(string, System.StringComparison)")]
	public extern static bool _0333a0fd5f67d8a0(string instance, string value, object comparisonType);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
	[Jazor(Op.Discard ,"string.StartsWith(string, bool, System.Globalization.CultureInfo)")]
	public extern static bool _16d66a076936ebd2(string instance, string value, bool ignoreCase, String? culture);

	///<summary>Determines whether this string instance starts with the specified character.</summary>
	[Jazor(Op.Discard ,"string.StartsWith(char)")]
	public extern static bool _ef46304ffa6d6ccf(string instance, Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified character array.</summary>
	[Jazor(Op.Discard ,"string.String(char[])")]
	public extern static string _6651b0a853e8e991(object value);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
	[Jazor(Op.Discard ,"string.String(char[], int, int)")]
	public extern static string _ddce1a944159fc8b(object value, Number startIndex, Number length);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
	[Jazor(Op.Discard ,"string.String(char, int)")]
	public extern static string _0ce0d88e18c041c8(Number c, Number count);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified read-only span.</summary>
	[Jazor(Op.Discard ,"string.String(System.ReadOnlySpan<char>)")]
	public extern static string _009fee2e166a416d(Uint32Array value);

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
	[Jazor(Op.Discard ,"static string.implicit operator System.ReadOnlySpan<char>(string)")]
	public extern static Uint32Array _5ff800b094791eb0();

	///<summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"string.Clone()")]
	public extern static object _488d7e5ec582c6fb(string instance);

	///<summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"static string.Copy(string)")]
	public extern static string _0dc0a16fd99401f8(string str);

	///<summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
	[Jazor(Op.Discard ,"string.CopyTo(int, char[], int, int)")]
	public extern static void _45bb6097c28a2f1e(string instance, Number sourceIndex, object destination, Number destinationIndex, Number count);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[Jazor(Op.Discard ,"string.CopyTo(System.Span<char>)")]
	public extern static void _2b86529e4a090aee(string instance, Uint32Array destination);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[Jazor(Op.Discard ,"string.TryCopyTo(System.Span<char>)")]
	public extern static bool _b0ab2eeef447828c(string instance, Uint32Array destination);

	///<summary>Copies the characters in this instance to a Unicode character array.</summary>
	[Jazor(Op.Discard ,"string.ToCharArray()")]
	public extern static char[] _7b8eb7b3d52c463d(string instance);

	///<summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
	[Jazor(Op.Discard ,"string.ToCharArray(int, int)")]
	public extern static char[] _53042938adf57f41(string instance, Number startIndex, Number length);

	/// <summary>
	/// C#: string.IsNullOrEmpty(value)
	/// JS: !value
	/// </summary>
	[Jazor(Op.Inline, "static string.IsNullOrEmpty(string)", "!@#{0}")]
	public extern static bool _f6e1cc63ac93e98f(string? value);

	/// <summary>
	/// C#: string.IsNullOrWhiteSpace(value)
	/// JS: !value?.trim()
	/// </summary>
	[Jazor(Op.Inline, "static string.IsNullOrWhiteSpace(string)", "!@#{0}?.trim()")]
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
	[Jazor(Op.Discard ,"string.ToString(System.IFormatProvider)")]
	public extern static string _555baf594c383de9(string instance, Intl.NumberFormat? provider);

	///<summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
	[Jazor(Op.Discard ,"string.GetEnumerator()")]
	public extern static System.CharEnumerator _b5d8c191b0b746ca(string instance);

	///<summary>Returns an enumeration of <see cref="T:System.Text.Rune" /> from this string.</summary>
	[Jazor(Op.Discard ,"string.EnumerateRunes()")]
	public extern static System.Text.StringRuneEnumerator _1e33e6a38a2179d0(string instance);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for the <see cref="T:System.String" /> class.</summary>
	[Jazor(Op.Discard ,"string.GetTypeCode()")]
	public extern static System.TypeCode _b4f593c93e2f2c61(string instance);

	///<summary>Indicates whether this string is in Unicode normalization form C.</summary>
	[Jazor(Op.Discard ,"string.IsNormalized()")]
	public extern static bool _f645a0207f41fd4a(string instance);

	///<summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
	[Jazor(Op.Discard ,"string.IsNormalized(System.Text.NormalizationForm)")]
	public extern static bool _30d0ce62702ae938(string instance, object normalizationForm);

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
	[Jazor(Op.Discard ,"string.Normalize()")]
	public extern static string _967ef647d59f3e39(string instance);

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
	[Jazor(Op.Discard ,"string.Normalize(System.Text.NormalizationForm)")]
	public extern static string _59b116010f03241b(string instance, object normalizationForm);

	/// <summary>
	/// C#: str[index]
	/// JS: str.charAt(index) or str[index]
	/// </summary>
	[Jazor(Op.Inline, "string.this[int].get", "@#{0}.charAt(@#{1})")]
	public extern static string _5ad63706a889c294(string instance, Number index);

	/// <summary>
	/// C#: str.Length
	/// JS: str.length
	/// </summary>
	[Jazor(Op.Replace, "string.Length.get", "length")]
	public extern static Number _1b0d64005dc28838(string instance);

	///<summary>Creates the string  representation of a specified object.</summary>
	[Jazor(Op.Discard ,"static string.Concat(object)")]
	public extern static string _db938b9c2eb90d32(object? arg0);

	///<summary>Concatenates the string representations of two specified objects.</summary>
	[Jazor(Op.Discard ,"static string.Concat(object, object)")]
	public extern static string _d330ca25546acf36(object? arg0, object? arg1);

	///<summary>Concatenates the string representations of three specified objects.</summary>
	[Jazor(Op.Discard ,"static string.Concat(object, object, object)")]
	public extern static string _dab9155adbef8f67(object? arg0, object? arg1, object? arg2);

	///<summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
	[Jazor(Op.Discard ,"static string.Concat(params object[])")]
	public extern static string _e102498b82e5b869( object args);

	///<summary>Concatenates the string representations of the elements in a specified span of objects.</summary>
	[Jazor(Op.Discard ,"static string.Concat(params System.ReadOnlySpan<object>)")]
	public extern static string _2d6a291b64a11ba3( object args);

	///<summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
	[Jazor(Op.Discard ,"static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)")]
	public extern static string _68574aee669f440f<T>(Array<T> values);

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"static string.Concat(System.Collections.Generic.IEnumerable<string>)")]
	public extern static string _a2a66aa54427416c(object values);

	/// <summary>
	/// C#: string.Concat(str0, str1)
	/// JS: str0 + str1
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string)", "(@#{0} + @#{1})")]
	public extern static string _021d71ef80d7918e(string? str0, string? str1);

	/// <summary>
	/// C#: string.Concat(str0, str1, str2)
	/// JS: str0 + str1 + str2
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string, string)", "(@#{0} + @#{1} + @#{2})")]
	public extern static string _ccc7897cb6f89406(string? str0, string? str1, string? str2);

	/// <summary>
	/// C#: string.Concat(str0, str1, str2, str3)
	/// JS: str0 + str1 + str2 + str3
	/// </summary>
	[Jazor(Op.Inline, "static string.Concat(string, string, string, string)", "(@#{0} + @#{1} + @#{2} + @#{3})")]
	public extern static string _abe4ba2b38df2f54(string? str0, string? str1, string? str2, string? str3);

	///<summary>Concatenates the string representations of two specified read-only character spans.</summary>
	[Jazor(Op.Discard ,"static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public extern static string _a6102c27abe1ff18(Uint32Array str0, Uint32Array str1);

	///<summary>Concatenates the string representations of three specified read-only character spans.</summary>
	[Jazor(Op.Discard ,"static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public extern static string _7de0cfb062a343ee(Uint32Array str0, Uint32Array str1, Uint32Array str2);

	///<summary>Concatenates the string representations of four specified read-only character spans.</summary>
	[Jazor(Op.Discard ,"static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public extern static string _5177ae056c5ca775(Uint32Array str0, Uint32Array str1, Uint32Array str2, Uint32Array str3);

	///<summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
	[Jazor(Op.Discard ,"static string.Concat(params string[])")]
	public extern static string _0f681227152a171b( object values);

	///<summary>Concatenates the elements of a specified span of <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"static string.Concat(params System.ReadOnlySpan<string>)")]
	public extern static string _22098d7fa5ce7a81( object values);

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

	///<summary>Returns a new string in which a specified string is inserted at a specified index position in this instance.</summary>
	[Jazor(Op.Discard ,"string.Insert(int, string)")]
	public extern static string _91223088dad76801(string instance, Number startIndex, string value);

	///<summary>Concatenates an array of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(char, params string[])")]
	public extern static string _14ec7ebbb72b7d13(Number separator,  object value);

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(char, params System.ReadOnlySpan<string>)")]
	public extern static string _9f939553178c2ca6(Number separator,  object value);

	///<summary>Concatenates all the elements of a string array, using the specified separator between each element.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, params string[])")]
	public extern static string _f269cd27a4bbd549(string? separator,  object value);

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, params System.ReadOnlySpan<string>)")]
	public extern static string _224682d778b9facf(string? separator,  object value);

	///<summary>Concatenates an array of strings, using the specified separator between each member, starting with the element in <paramref name="value" /> located at the <paramref name="startIndex" /> position, and concatenating up to <paramref name="count" /> elements.</summary>
	[Jazor(Op.Discard ,"static string.Join(char, string[], int, int)")]
	public extern static string _f461a3c632706317(Number separator, object value, Number startIndex, Number count);

	///<summary>Concatenates the specified elements of a string array, using the specified separator between each element.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, string[], int, int)")]
	public extern static string _f1ad756b7baec84b(string? separator, object value, Number startIndex, Number count);

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, System.Collections.Generic.IEnumerable<string>)")]
	public extern static string _d8814705c8078096(string? separator, object values);

	///<summary>Concatenates the string representations of an array of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(char, params object[])")]
	public extern static string _5ac0762c6816a423(Number separator,  object values);

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(char, params System.ReadOnlySpan<object>)")]
	public extern static string _477a1f45d63f93c2(Number separator,  object values);

	///<summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, params object[])")]
	public extern static string _c69ae51b8f3b72f0(string? separator,  object values);

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join(string, params System.ReadOnlySpan<object>)")]
	public extern static string _f8903c473c9e5f05(string? separator,  object values);

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)")]
	public extern static string _1c599eccbbc8f2b8<T>(Number separator, Array<T> values);

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)")]
	public extern static string _c78854b22e947a4f<T>(string? separator, Array<T> values);

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
	[Jazor(Op.Discard ,"string.PadLeft(int)")]
	public extern static string _26620c4bafb4f435(string instance, Number totalWidth);

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
	[Jazor(Op.Discard ,"string.PadLeft(int, char)")]
	public extern static string _7894e0294f780eb5(string instance, Number totalWidth, Number paddingChar);

	///<summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
	[Jazor(Op.Discard ,"string.PadRight(int)")]
	public extern static string _0e8f0a28fc1de8c2(string instance, Number totalWidth);

	///<summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
	[Jazor(Op.Discard ,"string.PadRight(int, char)")]
	public extern static string _685227781124d327(string instance, Number totalWidth, Number paddingChar);

	///<summary>Returns a new string in which a specified number of characters in the current instance beginning at a specified position have been deleted.</summary>
	[Jazor(Op.Discard ,"string.Remove(int, int)")]
	public extern static string _ac075983805231a6(string instance, Number startIndex, Number count);

	///<summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
	[Jazor(Op.Discard ,"string.Remove(int)")]
	public extern static string _d258363cef56cdfb(string instance, Number startIndex);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided culture and case sensitivity.</summary>
	[Jazor(Op.Discard ,"string.Replace(string, string, bool, System.Globalization.CultureInfo)")]
	public extern static string _80ebf2c83f8072e2(string instance, string oldValue, string? newValue, bool ignoreCase, String? culture);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.</summary>
	[Jazor(Op.Discard ,"string.Replace(string, string, System.StringComparison)")]
	public extern static string _8a7510653022a974(string instance, string oldValue, string? newValue, object comparisonType);

	///<summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
	[Jazor(Op.Discard ,"string.Replace(char, char)")]
	public extern static string _7d7cb13bbbbb83c8(string instance, Number oldChar, Number newChar);

	/// <summary>
	/// C#: str.Replace(oldValue, newValue)
	/// JS: str.replaceAll(oldValue, newValue)
	/// Note: Use replaceAll to replace all occurrences
	/// </summary>
	[Jazor(Op.Replace, "string.Replace(string, string)", "replaceAll")]
	public extern static string _78a0e353c29afbc9(string instance, string oldValue, string? newValue);

	///<summary>Replaces all newline sequences in the current string with <see cref="P:System.Environment.NewLine" />.</summary>
	[Jazor(Op.Discard ,"string.ReplaceLineEndings()")]
	public extern static string _3720e4de26fa4c1b(string instance);

	///<summary>Replaces all newline sequences in the current string with <paramref name="replacementText" />.</summary>
	[Jazor(Op.Discard ,"string.ReplaceLineEndings(string)")]
	public extern static string _35041c0250b36108(string instance, string replacementText);

	///<summary>Splits a string into substrings based on a specified delimiting character and, optionally, options.</summary>
	[Jazor(Op.Discard ,"string.Split(char, System.StringSplitOptions)")]
	public extern static string[] _d8080c573d45b4b4(string instance, Number separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting character and, optionally, options.        Splits a string into a maximum number of substrings based on the provided character separator, optionally omitting empty substrings from the result.</summary>
	[Jazor(Op.Discard ,"string.Split(char, int, System.StringSplitOptions)")]
	public extern static string[] _aaa73a4811837ec7(string instance, Number separator, Number count, object options);

	/// <summary>
	/// C#: str.Split(separator)
	/// JS: str.split(separator)
	/// Note: This is a simplified mapping, C# Split has more options
	/// </summary>
	[Jazor(Op.Replace, "string.Split(params char[])", "split")]
	public extern static string[] _62c8810ea13dba45(string instance, string separator);

	///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
	[Jazor(Op.Discard ,"string.Split(params System.ReadOnlySpan<char>)")]
	public extern static string[] _5417a93b3075813a(string instance,  Uint32Array separator);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters.</summary>
	[Jazor(Op.Discard ,"string.Split(char[], int)")]
	public extern static string[] _d03d120228c0c4ed(string instance, object separator, Number count);

	///<summary>Splits a string into substrings based on specified delimiting characters and options.</summary>
	[Jazor(Op.Discard ,"string.Split(char[], System.StringSplitOptions)")]
	public extern static string[] _25c1f15b0ed2cb6e(string instance, object separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters and, optionally, options.</summary>
	[Jazor(Op.Discard ,"string.Split(char[], int, System.StringSplitOptions)")]
	public extern static string[] _c8e5ceed33c6c638(string instance, object separator, Number count, object options);

	///<summary>Splits a string into substrings that are based on the provided string separator.</summary>
	[Jazor(Op.Discard ,"string.Split(string, System.StringSplitOptions)")]
	public extern static string[] _189761f781df8770(string instance, string? separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.</summary>
	[Jazor(Op.Discard ,"string.Split(string, int, System.StringSplitOptions)")]
	public extern static string[] _96eb0a23afa7fdfb(string instance, string? separator, Number count, object options);

	///<summary>Splits a string into substrings based on a specified delimiting string and, optionally, options.</summary>
	[Jazor(Op.Discard ,"string.Split(string[], System.StringSplitOptions)")]
	public extern static string[] _fff99c96206a241e(string instance, object separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting strings and, optionally, options.</summary>
	[Jazor(Op.Discard ,"string.Split(string[], int, System.StringSplitOptions)")]
	public extern static string[] _f3c7edcc7cc89a4a(string instance, object separator, Number count, object options);

	/// <summary>
	/// C#: str.Substring(startIndex)
	/// JS: str.substring(startIndex)
	/// </summary>
	[Jazor(Op.Replace, "string.Substring(int)", "substring")]
	public extern static string _6b947e3ae92ce851(string instance, Number startIndex);

	/// <summary>
	/// C#: str.Substring(startIndex, length)
	/// JS: str.substring(startIndex, startIndex + length)
	/// Note: C# Substring uses length, JS substring uses end index
	/// </summary>
	[Jazor(Op.Inline, "string.Substring(int, int)", "@#{0}.substring(@#{1}, @#{1} + @#{2})")]
	public extern static string _ac659b5819c0360c(string instance, Number startIndex, Number length);

	/// <summary>
	/// C#: str.ToLower()
	/// JS: str.toLowerCase()
	/// </summary>
	[Jazor(Op.Replace, "string.ToLower()", "toLowerCase")]
	public extern static string _482205d85705de41(string instance);

	///<summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
	[Jazor(Op.Discard ,"string.ToLower(System.Globalization.CultureInfo)")]
	public extern static string _8e06da9945efff04(string instance, String? culture);

	///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
	[Jazor(Op.Discard ,"string.ToLowerInvariant()")]
	public extern static string _3ff043d0307f4917(string instance);

	/// <summary>
	/// C#: str.ToUpper()
	/// JS: str.toUpperCase()
	/// </summary>
	[Jazor(Op.Replace, "string.ToUpper()", "toUpperCase")]
	public extern static string _4b84099d877364bd(string instance);

	///<summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
	[Jazor(Op.Discard ,"string.ToUpper(System.Globalization.CultureInfo)")]
	public extern static string _9369d4b370002404(string instance, String? culture);

	///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
	[Jazor(Op.Discard ,"string.ToUpperInvariant()")]
	public extern static string _3dc9c0782170eb46(string instance);

	/// <summary>
	/// C#: str.Trim()
	/// JS: str.trim()
	/// </summary>
	[Jazor(Op.Replace, "string.Trim()", "trim")]
	public extern static string _eb98ee79e16b7ad4(string instance);

	///<summary>Removes all leading and trailing instances of a character from the current string.</summary>
	[Jazor(Op.Discard ,"string.Trim(char)")]
	public extern static string _5d7e005b9dcb67de(string instance, Number trimChar);

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Discard ,"string.Trim(params char[])")]
	public extern static string _c6c444b4e71e14f7(string instance,  object trimChars);

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Discard ,"string.Trim(params System.ReadOnlySpan<char>)")]
	public extern static string _0e8e4169883e5222(string instance,  Uint32Array trimChars);

	/// <summary>
	/// C#: str.TrimStart()
	/// JS: str.trimStart()
	/// </summary>
	[Jazor(Op.Replace, "string.TrimStart()", "trimStart")]
	public extern static string _1ca7f6e7edd1e070(string instance);

	///<summary>Removes all the leading occurrences of a specified character from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimStart(char)")]
	public extern static string _561fe737e62cf332(string instance, Number trimChar);

	///<summary>Removes all the leading occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimStart(params char[])")]
	public extern static string _98731360726c6976(string instance,  object trimChars);

	///<summary>Removes all the leading occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimStart(params System.ReadOnlySpan<char>)")]
	public extern static string _f0473806a2e03bb6(string instance,  Uint32Array trimChars);

	/// <summary>
	/// C#: str.TrimEnd()
	/// JS: str.trimEnd()
	/// </summary>
	[Jazor(Op.Replace, "string.TrimEnd()", "trimEnd")]
	public extern static string _760bdb666072200b(string instance);

	///<summary>Removes all the trailing occurrences of a character from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimEnd(char)")]
	public extern static string _eb362a090d734099(string instance, Number trimChar);

	///<summary>Removes all the trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimEnd(params char[])")]
	public extern static string _a62862c1fbaa21c3(string instance,  object trimChars);

	///<summary>Removes all the trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[Jazor(Op.Discard ,"string.TrimEnd(params System.ReadOnlySpan<char>)")]
	public extern static string _4f8d256566de4b17(string instance,  Uint32Array trimChars);

	/// <summary>
	/// C#: str.Contains(value)
	/// JS: str.includes(value)
	/// </summary>
	[Jazor(Op.Replace, "string.Contains(string)", "includes")]
	public extern static bool _c42ed9bafadfb16c(string instance, string value);

	///<summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
	[Jazor(Op.Discard ,"string.Contains(string, System.StringComparison)")]
	public extern static bool _d52d7114d5c1b839(string instance, string value, object comparisonType);

	///<summary>Returns a value indicating whether a specified character occurs within this string.</summary>
	[Jazor(Op.Discard ,"string.Contains(char)")]
	public extern static bool _5de05262ccc56b2e(string instance, Number value);

	///<summary>Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.</summary>
	[Jazor(Op.Discard ,"string.Contains(char, System.StringComparison)")]
	public extern static bool _16d4b2b4de019fb2(string instance, Number value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(char)")]
	public extern static Number _9c8b4ffa28964fba(string instance, Number value);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(char, int)")]
	public extern static Number _c98394955f62f130(string instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. A parameter specifies the type of search to use for the specified character.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(char, System.StringComparison)")]
	public extern static Number _5331447e2c855a66(string instance, Number value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(char, int, int)")]
	public extern static Number _d2873e605fbed764(string instance, Number value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
	[Jazor(Op.Discard ,"string.IndexOfAny(char[])")]
	public extern static Number _69b749a1c6cbae78(string instance, object anyOf);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
	[Jazor(Op.Discard ,"string.IndexOfAny(char[], int)")]
	public extern static Number _63633a5f3b85c5a9(string instance, object anyOf, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.IndexOfAny(char[], int, int)")]
	public extern static Number _cb863079aae72451(string instance, object anyOf, Number startIndex, Number count);

	/// <summary>
	/// C#: str.IndexOf(value)
	/// JS: str.indexOf(value)
	/// </summary>
	[Jazor(Op.Replace, "string.IndexOf(string)", "indexOf")]
	public extern static Number _6fd03b0f0c2de338(string instance, string value);

	/// <summary>
	/// C#: str.IndexOf(value, startIndex)
	/// JS: str.indexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Replace, "string.IndexOf(string, int)", "indexOf")]
	public extern static Number _8c391718b5fbe536(string instance, string value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(string, int, int)")]
	public extern static Number _ff549d811898fb56(string instance, string value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(string, System.StringComparison)")]
	public extern static Number _3ae4900da2b07b27(string instance, string value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(string, int, System.StringComparison)")]
	public extern static Number _2fabe2b831abe71e(string instance, string value, Number startIndex, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
	[Jazor(Op.Discard ,"string.IndexOf(string, int, int, System.StringComparison)")]
	public extern static Number _ab22561fc42166db(string instance, string value, Number startIndex, Number count, object comparisonType);

	///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(char)")]
	public extern static Number _da9a8971cb787f7f(string instance, Number value);

	///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(char, int)")]
	public extern static Number _b21118cfc4c55581(string instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(char, int, int)")]
	public extern static Number _dbdd57f8d259ce66(string instance, Number value, Number startIndex, Number count);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOfAny(char[])")]
	public extern static Number _c0212f4213a99019(string instance, object anyOf);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOfAny(char[], int)")]
	public extern static Number _c401e64318e768c4(string instance, object anyOf, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOfAny(char[], int, int)")]
	public extern static Number _3c17fcef5615e7a3(string instance, object anyOf, Number startIndex, Number count);

	/// <summary>
	/// C#: str.LastIndexOf(value)
	/// JS: str.lastIndexOf(value)
	/// </summary>
	[Jazor(Op.Replace, "string.LastIndexOf(string)", "lastIndexOf")]
	public extern static Number _ed4ccee87d9df9fc(string instance, string value);

	/// <summary>
	/// C#: str.LastIndexOf(value, startIndex)
	/// JS: str.lastIndexOf(value, startIndex)
	/// </summary>
	[Jazor(Op.Replace, "string.LastIndexOf(string, int)", "lastIndexOf")]
	public extern static Number _404d5ed27b7e190a(string instance, string value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(string, int, int)")]
	public extern static Number _c4ee024d06ee238c(string instance, string value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(string, System.StringComparison)")]
	public extern static Number _78449c135e18c4bc(string instance, string value, object comparisonType);

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(string, int, System.StringComparison)")]
	public extern static Number _359dbce44ce4a4da(string instance, string value, Number startIndex, object comparisonType);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[Jazor(Op.Discard ,"string.LastIndexOf(string, int, int, System.StringComparison)")]
	public extern static Number _c911a06f021bd138(string instance, string value, Number startIndex, Number count, object comparisonType);
}
