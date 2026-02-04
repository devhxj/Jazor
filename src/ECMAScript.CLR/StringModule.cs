using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("string", WhiteListOp.Allowed, null,"System/StringModule.js")]
public static class StringModule
{
	///<summary>Represents the empty string. This field is read-only.</summary>
	[WhiteList("static readonly string.Empty", WhiteListOp.Discard)]
	public extern static String _b16f79dc7b155be3();

	///<summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Intern(string)", WhiteListOp.Discard)]
	public extern static string _1234444e218b96c3(object str);

	///<summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
	[WhiteList("static string.IsInterned(string)", WhiteListOp.Discard)]
	public extern static string? _0af8a50f6d6b3e26(object str);

	///<summary>Compares two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, string)", WhiteListOp.Discard)]
	public extern static Number _e16eea9fe3891a62(object strA, object strB);

	///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, string, bool)", WhiteListOp.Discard)]
	public extern static Number _20874c0b43640318(object strA, object strB, object ignoreCase);

	///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _9d940114ace1198f(object strA, object strB, object comparisonType);

	///<summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
	[WhiteList("static string.Compare(string, string, System.Globalization.CultureInfo, System.Globalization.CompareOptions)", WhiteListOp.Discard)]
	public extern static Number _3df4c7373f0b47b6(object strA, object strB, String? culture, object options);

	///<summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, string, bool, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static Number _7349ec2403e9750d(object strA, object strB, object ignoreCase, String? culture);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, int, string, int, int)", WhiteListOp.Discard)]
	public extern static Number _27da56ab23a965a9(object strA, Number indexA, object strB, Number indexB, Number length);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, int, string, int, int, bool)", WhiteListOp.Discard)]
	public extern static Number _ae9588dc995de641(object strA, Number indexA, object strB, Number indexB, Number length, object ignoreCase);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, int, string, int, int, bool, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static Number _e926c87c90eaf4a5(object strA, Number indexA, object strB, Number indexB, Number length, object ignoreCase, String? culture);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two substrings to each other in the sort order.</summary>
	[WhiteList("static string.Compare(string, int, string, int, int, System.Globalization.CultureInfo, System.Globalization.CompareOptions)", WhiteListOp.Discard)]
	public extern static Number _6de73d4e145d51a4(object strA, Number indexA, object strB, Number indexB, Number length, String? culture, object options);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	[WhiteList("static string.Compare(string, int, string, int, int, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _d78fb9d76fca75e4(object strA, Number indexA, object strB, Number indexB, Number length, object comparisonType);

	///<summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
	[WhiteList("static string.CompareOrdinal(string, string)", WhiteListOp.Discard)]
	public extern static Number _a55d307de6e31c7b(object strA, object strB);

	///<summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring.</summary>
	[WhiteList("static string.CompareOrdinal(string, int, string, int, int)", WhiteListOp.Discard)]
	public extern static Number _dc789454b6ef6bcb(object strA, Number indexA, object strB, Number indexB, Number length);

	///<summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	[WhiteList("string.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _629b0613344d82e7(String instance, Object? value);

	///<summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified string.</summary>
	[WhiteList("string.CompareTo(string)", WhiteListOp.Discard)]
	public extern static Number _380e7c7649d703f0(String instance, object strB);

	///<summary>Determines whether the end of this string instance matches the specified string.</summary>
	[WhiteList("string.EndsWith(string)", WhiteListOp.Discard)]
	public extern static bool _33de316681320ec7(String instance, object value);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[WhiteList("string.EndsWith(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _946b7129a48c8114(String instance, object value, object comparisonType);

	///<summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
	[WhiteList("string.EndsWith(string, bool, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static bool _679207cac049d3c6(String instance, object value, object ignoreCase, String? culture);

	///<summary>Determines whether the end of this string instance matches the specified character.</summary>
	[WhiteList("string.EndsWith(char)", WhiteListOp.Discard)]
	public extern static bool _7619ce4eda48c8e8(String instance, Number value);

	///<summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
	[WhiteList("override string.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _def18c2802a57249(String instance, Object? obj);

	///<summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
	[WhiteList("string.Equals(string)", WhiteListOp.Discard)]
	public extern static bool _6ee9bc86e4384225(String instance, object value);

	///<summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[WhiteList("string.Equals(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _f8e1e01e8c17e8bb(String instance, object value, object comparisonType);

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
	[WhiteList("static string.Equals(string, string)", WhiteListOp.Discard)]
	public extern static bool _e6b1648151c863d5(object a, object b);

	///<summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	[WhiteList("static string.Equals(string, string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _b7c36408f0f172e9(object a, object b, object comparisonType);

	///<summary>Determines whether two specified strings have the same value.</summary>
	[WhiteList("static string.operator ==(string, string)", WhiteListOp.Allowed)]
	public extern static bool _ee27dec45b308755(object a, object b);

	///<summary>Determines whether two specified strings have different values.</summary>
	[WhiteList("static string.operator !=(string, string)", WhiteListOp.Allowed)]
	public extern static bool _1573803c425863d3(object a, object b);

	///<summary>Returns the hash code for this string.</summary>
	[WhiteList("override string.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _bccdd3f386a6fbbc(String instance);

	///<summary>Returns the hash code for this string using the specified rules.</summary>
	[WhiteList("string.GetHashCode(System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _04edfc3090710ca7(String instance, object comparisonType);

	///<summary>Returns the hash code for the provided read-only character span.</summary>
	[WhiteList("static string.GetHashCode(System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static Number _4598a18be32f839d(Uint32Array value);

	///<summary>Returns the hash code for the provided read-only character span using the specified rules.</summary>
	[WhiteList("static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _d123047f69d911f5(Uint32Array value, object comparisonType);

	///<summary>Determines whether the beginning of this string instance matches the specified string.</summary>
	[WhiteList("string.StartsWith(string)", WhiteListOp.Discard)]
	public extern static bool _1cda198f8257d023(String instance, object value);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
	[WhiteList("string.StartsWith(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _0333a0fd5f67d8a0(String instance, object value, object comparisonType);

	///<summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
	[WhiteList("string.StartsWith(string, bool, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static bool _16d66a076936ebd2(String instance, object value, object ignoreCase, String? culture);

	///<summary>Determines whether this string instance starts with the specified character.</summary>
	[WhiteList("string.StartsWith(char)", WhiteListOp.Discard)]
	public extern static bool _ef46304ffa6d6ccf(String instance, Number value);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified character array.</summary>
	[WhiteList("string.String(char[])", WhiteListOp.Discard)]
	public extern static String _6651b0a853e8e991(object value);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
	[WhiteList("string.String(char[], int, int)", WhiteListOp.Discard)]
	public extern static String _ddce1a944159fc8b(object value, Number startIndex, Number length);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
	[WhiteList("string.String(char, int)", WhiteListOp.Discard)]
	public extern static String _0ce0d88e18c041c8(Number c, Number count);

	///<summary>Initializes a new instance of the <see cref="T:System.String" /> class to the Unicode characters indicated in the specified read-only span.</summary>
	[WhiteList("string.String(System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static String _009fee2e166a416d(Uint32Array value);

	///<summary>Creates a new string with a specific length and initializes it after creation by using the specified callback.</summary>
	[WhiteList("static string.Create<TState>(int, TState, System.Buffers.SpanAction<char, TState>)", WhiteListOp.Discard)]
	public extern static string _dcfb926861070414<TState>(Number length, object state, object action);

	///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
	[WhiteList("static string.Create(System.IFormatProvider, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)", WhiteListOp.Discard)]
	public extern static string _af610a42747a747c(Intl.NumberFormat? provider, Box<object> handler);

	///<summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
	[WhiteList("static string.Create(System.IFormatProvider, System.Span<char>, ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler)", WhiteListOp.Discard)]
	public extern static string _1978314137f5a599(Intl.NumberFormat? provider, Uint32Array initialBuffer, Box<object> handler);

	///<summary>Defines an implicit conversion of a given string to a read-only span of characters.</summary>
	[WhiteList("static string.implicit operator System.ReadOnlySpan<char>(string)", WhiteListOp.Discard)]
	public extern static Uint32Array _5ff800b094791eb0();

	///<summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
	[WhiteList("string.Clone()", WhiteListOp.Discard)]
	public extern static Object _488d7e5ec582c6fb(String instance);

	///<summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Copy(string)", WhiteListOp.Discard)]
	public extern static string _0dc0a16fd99401f8(object str);

	///<summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
	[WhiteList("string.CopyTo(int, char[], int, int)", WhiteListOp.Discard)]
	public extern static void _45bb6097c28a2f1e(String instance, Number sourceIndex, object destination, Number destinationIndex, Number count);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[WhiteList("string.CopyTo(System.Span<char>)", WhiteListOp.Discard)]
	public extern static void _2b86529e4a090aee(String instance, Uint32Array destination);

	///<summary>Copies the contents of this string into the destination span.</summary>
	[WhiteList("string.TryCopyTo(System.Span<char>)", WhiteListOp.Discard)]
	public extern static bool _b0ab2eeef447828c(String instance, Uint32Array destination);

	///<summary>Copies the characters in this instance to a Unicode character array.</summary>
	[WhiteList("string.ToCharArray()", WhiteListOp.Discard)]
	public extern static char[] _7b8eb7b3d52c463d(String instance);

	///<summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
	[WhiteList("string.ToCharArray(int, int)", WhiteListOp.Discard)]
	public extern static char[] _53042938adf57f41(String instance, Number startIndex, Number length);

	///<summary>Indicates whether the specified string is <see langword="null" /> or an empty string ("").</summary>
	[WhiteList("static string.IsNullOrEmpty(string)", WhiteListOp.Discard)]
	public extern static bool _f6e1cc63ac93e98f(object value);

	///<summary>Indicates whether a specified string is <see langword="null" />, empty, or consists only of white-space characters.</summary>
	[WhiteList("static string.IsNullOrWhiteSpace(string)", WhiteListOp.Discard)]
	public extern static bool _257a1a64b4d0f7d2(object value);

	///<summary>Returns a reference to the element of the string at index zero.This method is intended to support .NET compilers and is not intended to be called by user code.</summary>
	[WhiteList("string.GetPinnableReference()", WhiteListOp.Discard)]
	public extern static Number _519728f02e3ba627(String instance);

	///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	[WhiteList("override string.ToString()", WhiteListOp.Discard)]
	public extern static string _3158320a4854cc16(String instance);

	///<summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	[WhiteList("string.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _555baf594c383de9(String instance, Intl.NumberFormat? provider);

	///<summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
	[WhiteList("string.GetEnumerator()", WhiteListOp.Discard)]
	public extern static System.CharEnumerator _b5d8c191b0b746ca(String instance);

	///<summary>Returns an enumeration of <see cref="T:System.Text.Rune" /> from this string.</summary>
	[WhiteList("string.EnumerateRunes()", WhiteListOp.Discard)]
	public extern static System.Text.StringRuneEnumerator _1e33e6a38a2179d0(String instance);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for the <see cref="T:System.String" /> class.</summary>
	[WhiteList("string.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _b4f593c93e2f2c61(String instance);

	///<summary>Indicates whether this string is in Unicode normalization form C.</summary>
	[WhiteList("string.IsNormalized()", WhiteListOp.Discard)]
	public extern static bool _f645a0207f41fd4a(String instance);

	///<summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
	[WhiteList("string.IsNormalized(System.Text.NormalizationForm)", WhiteListOp.Discard)]
	public extern static bool _30d0ce62702ae938(String instance, object normalizationForm);

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
	[WhiteList("string.Normalize()", WhiteListOp.Discard)]
	public extern static string _967ef647d59f3e39(String instance);

	///<summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
	[WhiteList("string.Normalize(System.Text.NormalizationForm)", WhiteListOp.Discard)]
	public extern static string _59b116010f03241b(String instance, object normalizationForm);

	[WhiteList("string.this[int].get", WhiteListOp.Discard)]
	public extern static Number _5ad63706a889c294(String instance, Number index);

	[WhiteList("string.Length.get", WhiteListOp.Replace, "length")]
	public extern static Number _1b0d64005dc28838(String instance);

	///<summary>Creates the string  representation of a specified object.</summary>
	[WhiteList("static string.Concat(object)", WhiteListOp.Discard)]
	public extern static string _db938b9c2eb90d32(Object? arg0);

	///<summary>Concatenates the string representations of two specified objects.</summary>
	[WhiteList("static string.Concat(object, object)", WhiteListOp.Discard)]
	public extern static string _d330ca25546acf36(Object? arg0, Object? arg1);

	///<summary>Concatenates the string representations of three specified objects.</summary>
	[WhiteList("static string.Concat(object, object, object)", WhiteListOp.Discard)]
	public extern static string _dab9155adbef8f67(Object? arg0, Object? arg1, Object? arg2);

	///<summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
	[WhiteList("static string.Concat(params object[])", WhiteListOp.Discard)]
	public extern static string _e102498b82e5b869( object args);

	///<summary>Concatenates the string representations of the elements in a specified span of objects.</summary>
	[WhiteList("static string.Concat(params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _2d6a291b64a11ba3( object args);

	///<summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
	[WhiteList("static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)", WhiteListOp.Discard)]
	public extern static string _68574aee669f440f<T>(IEnumerable<T> values);

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Concat(System.Collections.Generic.IEnumerable<string>)", WhiteListOp.Discard)]
	public extern static string _a2a66aa54427416c(object values);

	///<summary>Concatenates two specified instances of <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Concat(string, string)", WhiteListOp.Replace, "concat")]
	public extern static string _021d71ef80d7918e(object str0, object str1);

	///<summary>Concatenates three specified instances of <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Concat(string, string, string)", WhiteListOp.Discard)]
	public extern static string _ccc7897cb6f89406(object str0, object str1, object str2);

	///<summary>Concatenates four specified instances of <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Concat(string, string, string, string)", WhiteListOp.Discard)]
	public extern static string _abe4ba2b38df2f54(object str0, object str1, object str2, object str3);

	///<summary>Concatenates the string representations of two specified read-only character spans.</summary>
	[WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _a6102c27abe1ff18(Uint32Array str0, Uint32Array str1);

	///<summary>Concatenates the string representations of three specified read-only character spans.</summary>
	[WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _7de0cfb062a343ee(Uint32Array str0, Uint32Array str1, Uint32Array str2);

	///<summary>Concatenates the string representations of four specified read-only character spans.</summary>
	[WhiteList("static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _5177ae056c5ca775(Uint32Array str0, Uint32Array str1, Uint32Array str2, Uint32Array str3);

	///<summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
	[WhiteList("static string.Concat(params string[])", WhiteListOp.Discard)]
	public extern static string _0f681227152a171b( object values);

	///<summary>Concatenates the elements of a specified span of <see cref="T:System.String" />.</summary>
	[WhiteList("static string.Concat(params System.ReadOnlySpan<string>)", WhiteListOp.Discard)]
	public extern static string _22098d7fa5ce7a81( object values);

	///<summary>Replaces one or more format items in a string with the string representation of a specified object.</summary>
	[WhiteList("static string.Format(string, object)", WhiteListOp.Discard)]
	public extern static string _980dff69bc3b8afa(object format, Object? arg0);

	///<summary>Replaces the format items in a string with the string representation of two specified objects.</summary>
	[WhiteList("static string.Format(string, object, object)", WhiteListOp.Discard)]
	public extern static string _8606f3cc36d1f8ed(object format, Object? arg0, Object? arg1);

	///<summary>Replaces the format items in a string with the string representation of three specified objects.</summary>
	[WhiteList("static string.Format(string, object, object, object)", WhiteListOp.Discard)]
	public extern static string _cda0978188193522(object format, Object? arg0, Object? arg1, Object? arg2);

	///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
	[WhiteList("static string.Format(string, params object[])", WhiteListOp.Discard)]
	public extern static string _99b8bed2ce27774c(object format,  object args);

	///<summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified span.</summary>
	[WhiteList("static string.Format(string, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _38dfe358e33e2c5d(object format,  object args);

	///<summary>Replaces the format item or items in a specified string with the string representation of the corresponding object. A parameter supplies culture-specific formatting information.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, string, object)", WhiteListOp.Discard)]
	public extern static string _03246c01949cf478(Intl.NumberFormat? provider, object format, Object? arg0);

	///<summary>Replaces the format items in a string with the string representation of two specified objects. A parameter supplies culture-specific formatting information.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, string, object, object)", WhiteListOp.Discard)]
	public extern static string _661214177662ec13(Intl.NumberFormat? provider, object format, Object? arg0, Object? arg1);

	///<summary>Replaces the format items in a string with the string representation of three specified objects. An parameter supplies culture-specific formatting information.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, string, object, object, object)", WhiteListOp.Discard)]
	public extern static string _915cdc23ed4c4425(Intl.NumberFormat? provider, object format, Object? arg0, Object? arg1, Object? arg2);

	///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, string, params object[])", WhiteListOp.Discard)]
	public extern static string _2b199e5bf9c94fc2(Intl.NumberFormat? provider, object format,  object args);

	///<summary>Replaces the format items in a string with the string representations of corresponding objects in a specified span. A parameter supplies culture-specific formatting information.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, string, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _8a09a1f92212621f(Intl.NumberFormat? provider, object format,  object args);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[WhiteList("static string.Format<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)", WhiteListOp.Discard)]
	public extern static string _2fd17baa6bc57571<TArg0>(Intl.NumberFormat? provider, object format, object arg0);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[WhiteList("static string.Format<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)", WhiteListOp.Discard)]
	public extern static string _879b6befd667cd5c<TArg0, TArg1>(Intl.NumberFormat? provider, object format, object arg0, object arg1);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[WhiteList("static string.Format<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)", WhiteListOp.Discard)]
	public extern static string _850c49e163cd3ed0<TArg0, TArg1, TArg2>(Intl.NumberFormat? provider, object format, object arg0, object arg1, object arg2);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params object[])", WhiteListOp.Discard)]
	public extern static string _1183035ecb38f2a4(Intl.NumberFormat? provider, object format,  object args);

	///<summary>Replaces the format item or items in a <see cref="T:System.Text.CompositeFormat" /> with the string representation of the corresponding objects in the specified format.</summary>
	[WhiteList("static string.Format(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _e4458a04839fcdc5(Intl.NumberFormat? provider, object format,  object args);

	///<summary>Returns a new string in which a specified string is inserted at a specified index position in this instance.</summary>
	[WhiteList("string.Insert(int, string)", WhiteListOp.Discard)]
	public extern static string _91223088dad76801(String instance, Number startIndex, object value);

	///<summary>Concatenates an array of strings, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(char, params string[])", WhiteListOp.Discard)]
	public extern static string _14ec7ebbb72b7d13(Number separator,  object value);

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(char, params System.ReadOnlySpan<string>)", WhiteListOp.Discard)]
	public extern static string _9f939553178c2ca6(Number separator,  object value);

	///<summary>Concatenates all the elements of a string array, using the specified separator between each element.</summary>
	[WhiteList("static string.Join(string, params string[])", WhiteListOp.Replace, "join")]
	public extern static string _f269cd27a4bbd549(object separator,  object value);

	///<summary>Concatenates a span of strings, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(string, params System.ReadOnlySpan<string>)", WhiteListOp.Discard)]
	public extern static string _224682d778b9facf(object separator,  object value);

	///<summary>Concatenates an array of strings, using the specified separator between each member, starting with the element in <paramref name="value" /> located at the <paramref name="startIndex" /> position, and concatenating up to <paramref name="count" /> elements.</summary>
	[WhiteList("static string.Join(char, string[], int, int)", WhiteListOp.Discard)]
	public extern static string _f461a3c632706317(Number separator, object value, Number startIndex, Number count);

	///<summary>Concatenates the specified elements of a string array, using the specified separator between each element.</summary>
	[WhiteList("static string.Join(string, string[], int, int)", WhiteListOp.Discard)]
	public extern static string _f1ad756b7baec84b(object separator, object value, Number startIndex, Number count);

	///<summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(string, System.Collections.Generic.IEnumerable<string>)", WhiteListOp.Discard)]
	public extern static string _d8814705c8078096(object separator, object values);

	///<summary>Concatenates the string representations of an array of objects, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(char, params object[])", WhiteListOp.Discard)]
	public extern static string _5ac0762c6816a423(Number separator,  object values);

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(char, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _477a1f45d63f93c2(Number separator,  object values);

	///<summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
	[WhiteList("static string.Join(string, params object[])", WhiteListOp.Discard)]
	public extern static string _c69ae51b8f3b72f0(object separator,  object values);

	///<summary>Concatenates the string representations of a span of objects, using the specified separator between each member.</summary>
	[WhiteList("static string.Join(string, params System.ReadOnlySpan<object>)", WhiteListOp.Discard)]
	public extern static string _f8903c473c9e5f05(object separator,  object values);

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[WhiteList("static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)", WhiteListOp.Discard)]
	public extern static string _1c599eccbbc8f2b8<T>(Number separator, IEnumerable<T> values);

	///<summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	[WhiteList("static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)", WhiteListOp.Discard)]
	public extern static string _c78854b22e947a4f<T>(object separator, IEnumerable<T> values);

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
	[WhiteList("string.PadLeft(int)", WhiteListOp.Discard)]
	public extern static string _26620c4bafb4f435(String instance, Number totalWidth);

	///<summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
	[WhiteList("string.PadLeft(int, char)", WhiteListOp.Discard)]
	public extern static string _7894e0294f780eb5(String instance, Number totalWidth, Number paddingChar);

	///<summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
	[WhiteList("string.PadRight(int)", WhiteListOp.Discard)]
	public extern static string _0e8f0a28fc1de8c2(String instance, Number totalWidth);

	///<summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
	[WhiteList("string.PadRight(int, char)", WhiteListOp.Discard)]
	public extern static string _685227781124d327(String instance, Number totalWidth, Number paddingChar);

	///<summary>Returns a new string in which a specified number of characters in the current instance beginning at a specified position have been deleted.</summary>
	[WhiteList("string.Remove(int, int)", WhiteListOp.Discard)]
	public extern static string _ac075983805231a6(String instance, Number startIndex, Number count);

	///<summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
	[WhiteList("string.Remove(int)", WhiteListOp.Discard)]
	public extern static string _d258363cef56cdfb(String instance, Number startIndex);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided culture and case sensitivity.</summary>
	[WhiteList("string.Replace(string, string, bool, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static string _80ebf2c83f8072e2(String instance, object oldValue, object newValue, object ignoreCase, String? culture);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.</summary>
	[WhiteList("string.Replace(string, string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static string _8a7510653022a974(String instance, object oldValue, object newValue, object comparisonType);

	///<summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
	[WhiteList("string.Replace(char, char)", WhiteListOp.Discard)]
	public extern static string _7d7cb13bbbbb83c8(String instance, Number oldChar, Number newChar);

	///<summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.</summary>
	[WhiteList("string.Replace(string, string)", WhiteListOp.Discard)]
	public extern static string _78a0e353c29afbc9(String instance, object oldValue, object newValue);

	///<summary>Replaces all newline sequences in the current string with <see cref="P:System.Environment.NewLine" />.</summary>
	[WhiteList("string.ReplaceLineEndings()", WhiteListOp.Discard)]
	public extern static string _3720e4de26fa4c1b(String instance);

	///<summary>Replaces all newline sequences in the current string with <paramref name="replacementText" />.</summary>
	[WhiteList("string.ReplaceLineEndings(string)", WhiteListOp.Discard)]
	public extern static string _35041c0250b36108(String instance, object replacementText);

	///<summary>Splits a string into substrings based on a specified delimiting character and, optionally, options.</summary>
	[WhiteList("string.Split(char, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _d8080c573d45b4b4(String instance, Number separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting character and, optionally, options.        Splits a string into a maximum number of substrings based on the provided character separator, optionally omitting empty substrings from the result.</summary>
	[WhiteList("string.Split(char, int, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _aaa73a4811837ec7(String instance, Number separator, Number count, object options);

	///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
	[WhiteList("string.Split(params char[])", WhiteListOp.Discard)]
	public extern static string[] _62c8810ea13dba45(String instance,  object separator);

	///<summary>Splits a string into substrings based on specified delimiting characters.</summary>
	[WhiteList("string.Split(params System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string[] _5417a93b3075813a(String instance,  Uint32Array separator);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters.</summary>
	[WhiteList("string.Split(char[], int)", WhiteListOp.Discard)]
	public extern static string[] _d03d120228c0c4ed(String instance, object separator, Number count);

	///<summary>Splits a string into substrings based on specified delimiting characters and options.</summary>
	[WhiteList("string.Split(char[], System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _25c1f15b0ed2cb6e(String instance, object separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting characters and, optionally, options.</summary>
	[WhiteList("string.Split(char[], int, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _c8e5ceed33c6c638(String instance, object separator, Number count, object options);

	///<summary>Splits a string into substrings that are based on the provided string separator.</summary>
	[WhiteList("string.Split(string, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _189761f781df8770(String instance, object separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.</summary>
	[WhiteList("string.Split(string, int, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _96eb0a23afa7fdfb(String instance, object separator, Number count, object options);

	///<summary>Splits a string into substrings based on a specified delimiting string and, optionally, options.</summary>
	[WhiteList("string.Split(string[], System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _fff99c96206a241e(String instance, object separator, object options);

	///<summary>Splits a string into a maximum number of substrings based on specified delimiting strings and, optionally, options.</summary>
	[WhiteList("string.Split(string[], int, System.StringSplitOptions)", WhiteListOp.Discard)]
	public extern static string[] _f3c7edcc7cc89a4a(String instance, object separator, Number count, object options);

	///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.</summary>
	[WhiteList("string.Substring(int)", WhiteListOp.Discard)]
	public extern static string _6b947e3ae92ce851(String instance, Number startIndex);

	///<summary>Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.</summary>
	[WhiteList("string.Substring(int, int)", WhiteListOp.Replace, "substr")]
	public extern static string _ac659b5819c0360c(String instance, Number startIndex, Number length);

	///<summary>Returns a copy of this string converted to lowercase.</summary>
	[WhiteList("string.ToLower()", WhiteListOp.Replace, "toLowerCase")]
	public extern static string _482205d85705de41(String instance);

	///<summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
	[WhiteList("string.ToLower(System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static string _8e06da9945efff04(String instance, String? culture);

	///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
	[WhiteList("string.ToLowerInvariant()", WhiteListOp.Replace, "toLowerCase")]
	public extern static string _3ff043d0307f4917(String instance);

	///<summary>Returns a copy of this string converted to uppercase.</summary>
	[WhiteList("string.ToUpper()", WhiteListOp.Replace, "toUpperCase")]
	public extern static string _4b84099d877364bd(String instance);

	///<summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
	[WhiteList("string.ToUpper(System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static string _9369d4b370002404(String instance, String? culture);

	///<summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
	[WhiteList("string.ToUpperInvariant()", WhiteListOp.Replace, "toUpperCase")]
	public extern static string _3dc9c0782170eb46(String instance);

	///<summary>Removes all leading and trailing white-space characters from the current string.</summary>
	[WhiteList("string.Trim()", WhiteListOp.Replace, "trim")]
	public extern static string _eb98ee79e16b7ad4(String instance);

	///<summary>Removes all leading and trailing instances of a character from the current string.</summary>
	[WhiteList("string.Trim(char)", WhiteListOp.Discard)]
	public extern static string _5d7e005b9dcb67de(String instance, Number trimChar);

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[WhiteList("string.Trim(params char[])", WhiteListOp.Discard)]
	public extern static string _c6c444b4e71e14f7(String instance,  object trimChars);

	///<summary>Removes all leading and trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[WhiteList("string.Trim(params System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _0e8e4169883e5222(String instance,  Uint32Array trimChars);

	///<summary>Removes all the leading white-space characters from the current string.</summary>
	[WhiteList("string.TrimStart()", WhiteListOp.Replace, "trimStart")]
	public extern static string _1ca7f6e7edd1e070(String instance);

	///<summary>Removes all the leading occurrences of a specified character from the current string.</summary>
	[WhiteList("string.TrimStart(char)", WhiteListOp.Discard)]
	public extern static string _561fe737e62cf332(String instance, Number trimChar);

	///<summary>Removes all the leading occurrences of a set of characters specified in an array from the current string.</summary>
	[WhiteList("string.TrimStart(params char[])", WhiteListOp.Discard)]
	public extern static string _98731360726c6976(String instance,  object trimChars);

	///<summary>Removes all the leading occurrences of a set of characters specified in a span from the current string.</summary>
	[WhiteList("string.TrimStart(params System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _f0473806a2e03bb6(String instance,  Uint32Array trimChars);

	///<summary>Removes all the trailing white-space characters from the current string.</summary>
	[WhiteList("string.TrimEnd()", WhiteListOp.Replace, "trimEnd")]
	public extern static string _760bdb666072200b(String instance);

	///<summary>Removes all the trailing occurrences of a character from the current string.</summary>
	[WhiteList("string.TrimEnd(char)", WhiteListOp.Discard)]
	public extern static string _eb362a090d734099(String instance, Number trimChar);

	///<summary>Removes all the trailing occurrences of a set of characters specified in an array from the current string.</summary>
	[WhiteList("string.TrimEnd(params char[])", WhiteListOp.Discard)]
	public extern static string _a62862c1fbaa21c3(String instance,  object trimChars);

	///<summary>Removes all the trailing occurrences of a set of characters specified in a span from the current string.</summary>
	[WhiteList("string.TrimEnd(params System.ReadOnlySpan<char>)", WhiteListOp.Discard)]
	public extern static string _4f8d256566de4b17(String instance,  Uint32Array trimChars);

	///<summary>Returns a value indicating whether a specified substring occurs within this string.</summary>
	[WhiteList("string.Contains(string)", WhiteListOp.Replace, "includes")]
	public extern static bool _c42ed9bafadfb16c(String instance, object value);

	///<summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
	[WhiteList("string.Contains(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _d52d7114d5c1b839(String instance, object value, object comparisonType);

	///<summary>Returns a value indicating whether a specified character occurs within this string.</summary>
	[WhiteList("string.Contains(char)", WhiteListOp.Replace, "includes")]
	public extern static bool _5de05262ccc56b2e(String instance, Number value);

	///<summary>Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.</summary>
	[WhiteList("string.Contains(char, System.StringComparison)", WhiteListOp.Discard)]
	public extern static bool _16d4b2b4de019fb2(String instance, Number value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
	[WhiteList("string.IndexOf(char)", WhiteListOp.Replace, "indexOf")]
	public extern static Number _9c8b4ffa28964fba(String instance, Number value);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
	[WhiteList("string.IndexOf(char, int)", WhiteListOp.Discard)]
	public extern static Number _c98394955f62f130(String instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. A parameter specifies the type of search to use for the specified character.</summary>
	[WhiteList("string.IndexOf(char, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _5331447e2c855a66(String instance, Number value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[WhiteList("string.IndexOf(char, int, int)", WhiteListOp.Discard)]
	public extern static Number _d2873e605fbed764(String instance, Number value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
	[WhiteList("string.IndexOfAny(char[])", WhiteListOp.Discard)]
	public extern static Number _69b749a1c6cbae78(String instance, object anyOf);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
	[WhiteList("string.IndexOfAny(char[], int)", WhiteListOp.Discard)]
	public extern static Number _63633a5f3b85c5a9(String instance, object anyOf, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[WhiteList("string.IndexOfAny(char[], int, int)", WhiteListOp.Discard)]
	public extern static Number _cb863079aae72451(String instance, object anyOf, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance.</summary>
	[WhiteList("string.IndexOf(string)", WhiteListOp.Discard)]
	public extern static Number _6fd03b0f0c2de338(String instance, object value);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position.</summary>
	[WhiteList("string.IndexOf(string, int)", WhiteListOp.Discard)]
	public extern static Number _8c391718b5fbe536(String instance, object value, Number startIndex);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	[WhiteList("string.IndexOf(string, int, int)", WhiteListOp.Discard)]
	public extern static Number _ff549d811898fb56(String instance, object value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[WhiteList("string.IndexOf(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _3ae4900da2b07b27(String instance, object value, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
	[WhiteList("string.IndexOf(string, int, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _2fabe2b831abe71e(String instance, object value, Number startIndex, object comparisonType);

	///<summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
	[WhiteList("string.IndexOf(string, int, int, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _ab22561fc42166db(String instance, object value, Number startIndex, Number count, object comparisonType);

	///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
	[WhiteList("string.LastIndexOf(char)", WhiteListOp.Replace, "lastIndexOf")]
	public extern static Number _da9a8971cb787f7f(String instance, Number value);

	///<summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[WhiteList("string.LastIndexOf(char, int)", WhiteListOp.Discard)]
	public extern static Number _b21118cfc4c55581(String instance, Number value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[WhiteList("string.LastIndexOf(char, int, int)", WhiteListOp.Discard)]
	public extern static Number _dbdd57f8d259ce66(String instance, Number value, Number startIndex, Number count);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
	[WhiteList("string.LastIndexOfAny(char[])", WhiteListOp.Discard)]
	public extern static Number _c0212f4213a99019(String instance, object anyOf);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[WhiteList("string.LastIndexOfAny(char[], int)", WhiteListOp.Discard)]
	public extern static Number _c401e64318e768c4(String instance, object anyOf, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[WhiteList("string.LastIndexOfAny(char[], int, int)", WhiteListOp.Discard)]
	public extern static Number _3c17fcef5615e7a3(String instance, object anyOf, Number startIndex, Number count);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance.</summary>
	[WhiteList("string.LastIndexOf(string)", WhiteListOp.Discard)]
	public extern static Number _ed4ccee87d9df9fc(String instance, object value);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	[WhiteList("string.LastIndexOf(string, int)", WhiteListOp.Discard)]
	public extern static Number _404d5ed27b7e190a(String instance, object value, Number startIndex);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	[WhiteList("string.LastIndexOf(string, int, int)", WhiteListOp.Discard)]
	public extern static Number _c4ee024d06ee238c(String instance, object value, Number startIndex, Number count);

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	[WhiteList("string.LastIndexOf(string, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _78449c135e18c4bc(String instance, object value, object comparisonType);

	///<summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[WhiteList("string.LastIndexOf(string, int, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _359dbce44ce4a4da(String instance, object value, Number startIndex, object comparisonType);

	///<summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	[WhiteList("string.LastIndexOf(string, int, int, System.StringComparison)", WhiteListOp.Discard)]
	public extern static Number _c911a06f021bd138(String instance, object value, Number startIndex, Number count, object comparisonType);
}
