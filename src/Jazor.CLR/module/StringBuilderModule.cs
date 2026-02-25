namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Text.StringBuilder","System/Text/StringBuilderModule.js")]
public static class StringBuilderModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder()")]
	public extern static System.Text.StringBuilder _2154365d1f9a2abf();

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified capacity.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder(int)")]
	public extern static System.Text.StringBuilder _404c94878c905b27(Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder(string)")]
	public extern static System.Text.StringBuilder _c2c8c4778873ccdc(string? value);

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string and capacity.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder(string, int)")]
	public extern static System.Text.StringBuilder _8ddc5378f62c27cc(string? value, Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class from the specified substring and capacity.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder(string, int, int, int)")]
	public extern static System.Text.StringBuilder _70c61ab8ef3313c3(string? value, Number startIndex, Number length, Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class that starts with a specified capacity and can grow to a specified maximum.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.StringBuilder(int, int)")]
	public extern static System.Text.StringBuilder _f69cee28dea8bcdc(Number capacity, Number maxCapacity);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.Capacity.get")]
	public extern static Number _20274b0eadfc0539(System.Text.StringBuilder instance);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.Capacity.set")]
	public extern static void _d58ab6215b243f4f(System.Text.StringBuilder instance, Number value);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.MaxCapacity.get")]
	public extern static Number _32a883f2233e3134(System.Text.StringBuilder instance);

	///<summary>Ensures that the capacity of this instance of <see cref="T:System.Text.StringBuilder" /> is at least the specified value.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.EnsureCapacity(int)")]
	public extern static Number _e957bcfaa166161c(System.Text.StringBuilder instance, Number capacity);

	///<summary>Converts the value of this instance to a <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"override System.Text.StringBuilder.ToString()")]
	public extern static string _010347a06fe9584c(System.Text.StringBuilder instance);

	///<summary>Converts the value of a substring of this instance to a <see cref="T:System.String" />.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.ToString(int, int)")]
	public extern static string _4941946dde4f03f0(System.Text.StringBuilder instance, Number startIndex, Number length);

	///<summary>Removes all characters from the current <see cref="T:System.Text.StringBuilder" /> instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Clear()")]
	public extern static System.Text.StringBuilder _3b8e77fc2c4d5f63(System.Text.StringBuilder instance);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.Length.get")]
	public extern static Number _76a78d5aa26cb6e0(System.Text.StringBuilder instance);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.Length.set")]
	public extern static void _085925374c6d3abd(System.Text.StringBuilder instance, Number value);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.this[int].get")]
	public extern static Number _c59f10eccb1d75d4(System.Text.StringBuilder instance, Number index);

	[Jazor(Op.Discard ,"System.Text.StringBuilder.this[int].set")]
	public extern static void _a970d620cd814959(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Returns an object that can be used to iterate through the chunks of characters represented in a <see langword="ReadOnlyMemory&lt;Char&gt;" /> created from this <see cref="T:System.Text.StringBuilder" /> instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.GetChunks()")]
	public extern static System.Text.StringBuilder.ChunkEnumerator _eb70112718b443d3(System.Text.StringBuilder instance);

	///<summary>Appends a specified number of copies of the string representation of a Unicode character to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(char, int)")]
	public extern static System.Text.StringBuilder _77869f53e4b4cf63(System.Text.StringBuilder instance, Number value, Number repeatCount);

	///<summary>Appends the string representation of a specified subarray of Unicode characters to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(char[], int, int)")]
	public extern static System.Text.StringBuilder _76a6be47564b1442(System.Text.StringBuilder instance, object value, Number startIndex, Number charCount);

	///<summary>Appends a copy of the specified string to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(string)")]
	public extern static System.Text.StringBuilder _2879b76db56f25fb(System.Text.StringBuilder instance, string? value);

	///<summary>Appends a copy of a specified substring to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(string, int, int)")]
	public extern static System.Text.StringBuilder _643a38ba616afd42(System.Text.StringBuilder instance, string? value, Number startIndex, Number count);

	///<summary>Appends the string representation of a specified string builder to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.Text.StringBuilder)")]
	public extern static System.Text.StringBuilder _390481e4ef6d1b43(System.Text.StringBuilder instance, object value);

	///<summary>Appends a copy of a substring within a specified string builder to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)")]
	public extern static System.Text.StringBuilder _2a75c7a6bec12592(System.Text.StringBuilder instance, object value, Number startIndex, Number count);

	///<summary>Appends the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine()")]
	public extern static System.Text.StringBuilder _35fe8bcf463e879b(System.Text.StringBuilder instance);

	///<summary>Appends a copy of the specified string followed by the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine(string)")]
	public extern static System.Text.StringBuilder _c06aaa44e213e405(System.Text.StringBuilder instance, string? value);

	///<summary>Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.CopyTo(int, char[], int, int)")]
	public extern static void _e7c76d547b84e1dd(System.Text.StringBuilder instance, Number sourceIndex, object destination, Number destinationIndex, Number count);

	///<summary>Copies the characters from a specified segment of this instance to a destination <see cref="T:System.Char" /> span.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.CopyTo(int, System.Span<char>, int)")]
	public extern static void _54205e7ac737a01c(System.Text.StringBuilder instance, Number sourceIndex, Uint32Array destination, Number count);

	///<summary>Inserts one or more copies of a specified string into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, string, int)")]
	public extern static System.Text.StringBuilder _da897479d9bd6139(System.Text.StringBuilder instance, Number index, string? value, Number count);

	///<summary>Removes the specified range of characters from this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Remove(int, int)")]
	public extern static System.Text.StringBuilder _152bf60dc35a5bb6(System.Text.StringBuilder instance, Number startIndex, Number length);

	///<summary>Appends the string representation of a specified Boolean value to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(bool)")]
	public extern static System.Text.StringBuilder _dded353c61620d12(System.Text.StringBuilder instance, object value);

	///<summary>Appends the string representation of a specified <see cref="T:System.Char" /> object to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(char)")]
	public extern static System.Text.StringBuilder _a2ce7c5adfc1553c(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 8-bit signed integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(sbyte)")]
	public extern static System.Text.StringBuilder _3ce4c9341fd5777f(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 8-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(byte)")]
	public extern static System.Text.StringBuilder _d530c416b64aac49(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 16-bit signed integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(short)")]
	public extern static System.Text.StringBuilder _ea789609ea3aeeb0(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 32-bit signed integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(int)")]
	public extern static System.Text.StringBuilder _212b9738d2ea3b2d(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 64-bit signed integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(long)")]
	public extern static System.Text.StringBuilder _a20035534ee530dd(System.Text.StringBuilder instance, BigInt value);

	///<summary>Appends the string representation of a specified single-precision floating-point number to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(float)")]
	public extern static System.Text.StringBuilder _ec1b541b6a274b24(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified double-precision floating-point number to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(double)")]
	public extern static System.Text.StringBuilder _817e46ee3d60bf66(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified decimal number to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(decimal)")]
	public extern static System.Text.StringBuilder _f07022820ca3881f(System.Text.StringBuilder instance, string value);

	///<summary>Appends the string representation of a specified 16-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(ushort)")]
	public extern static System.Text.StringBuilder _37e94b64bce60492(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 32-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(uint)")]
	public extern static System.Text.StringBuilder _423a4a09f9fa54c4(System.Text.StringBuilder instance, Number value);

	///<summary>Appends the string representation of a specified 64-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(ulong)")]
	public extern static System.Text.StringBuilder _f09314f07502e2a3(System.Text.StringBuilder instance, BigInt value);

	///<summary>Appends the string representation of a specified object to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(object)")]
	public extern static System.Text.StringBuilder _06379efa8addb10d(System.Text.StringBuilder instance, object? value);

	///<summary>Appends the string representation of the Unicode characters in a specified array to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(char[])")]
	public extern static System.Text.StringBuilder _4ec74831297581ec(System.Text.StringBuilder instance, object value);

	///<summary>Appends the string representation of a specified read-only character span to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)")]
	public extern static System.Text.StringBuilder _8c68c811d3d42bcf(System.Text.StringBuilder instance, Uint32Array value);

	///<summary>Appends the string representation of a specified read-only character memory region to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.ReadOnlyMemory<char>)")]
	public extern static System.Text.StringBuilder _19e34431ab825546(System.Text.StringBuilder instance, object value);

	///<summary>Appends the specified interpolated string to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _b753ce137296837a(System.Text.StringBuilder instance, ref object handler);

	///<summary>Appends the specified interpolated string to this instance using the specified format.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _c38a3237ddfa0a19(System.Text.StringBuilder instance, Intl.NumberFormat? provider, ref object handler);

	///<summary>Appends the specified interpolated string followed by the default line terminator to the end of the current StringBuilder object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _c52ed5039c53253f(System.Text.StringBuilder instance, ref object handler);

	///<summary>Appends the specified interpolated string using the specified format, followed by the default line terminator, to the end of the current StringBuilder object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _0192e43c680249a7(System.Text.StringBuilder instance, Intl.NumberFormat? provider, ref object handler);

	///<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(string, params object[])")]
	public extern static System.Text.StringBuilder _8bc8cc43c6d93195(System.Text.StringBuilder instance, string? separator,  object values);

	///<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)")]
	public extern static System.Text.StringBuilder _f4377679fddd51ad(System.Text.StringBuilder instance, string? separator,  object values);

	///<summary>Concatenates and appends the members of a collection, using the specified separator between each member.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)")]
	public extern static System.Text.StringBuilder _8d04089684a00c7b<T>(System.Text.StringBuilder instance, string? separator, Array<T> values);

	///<summary>Concatenates the strings of the provided array, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(string, params string[])")]
	public extern static System.Text.StringBuilder _6ceea7a4bfd233b6(System.Text.StringBuilder instance, string? separator,  object values);

	///<summary>Concatenates the strings of the provided span, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)")]
	public extern static System.Text.StringBuilder _035c615b56218700(System.Text.StringBuilder instance, string? separator,  object values);

	///<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(char, params object[])")]
	public extern static System.Text.StringBuilder _a5aab658026ac255(System.Text.StringBuilder instance, Number separator,  object values);

	///<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)")]
	public extern static System.Text.StringBuilder _f9ca702aaa0e6322(System.Text.StringBuilder instance, Number separator,  object values);

	///<summary>Concatenates and appends the members of a collection, using the specified char separator between each member.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)")]
	public extern static System.Text.StringBuilder _3510fcab582042e0<T>(System.Text.StringBuilder instance, Number separator, Array<T> values);

	///<summary>Concatenates the strings of the provided array, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(char, params string[])")]
	public extern static System.Text.StringBuilder _02a3ec9f0e91877f(System.Text.StringBuilder instance, Number separator,  object values);

	///<summary>Concatenates the strings of the provided span, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)")]
	public extern static System.Text.StringBuilder _08c4f86d45c8b851(System.Text.StringBuilder instance, Number separator,  object values);

	///<summary>Inserts a string into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, string)")]
	public extern static System.Text.StringBuilder _40a305d0112c40d9(System.Text.StringBuilder instance, Number index, string? value);

	///<summary>Inserts the string representation of a Boolean value into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, bool)")]
	public extern static System.Text.StringBuilder _2e7808d3cd4780e8(System.Text.StringBuilder instance, Number index, object value);

	///<summary>Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, sbyte)")]
	public extern static System.Text.StringBuilder _5d866e86d8040d7d(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, byte)")]
	public extern static System.Text.StringBuilder _a90cbae6c991fb88(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, short)")]
	public extern static System.Text.StringBuilder _bf04d5cd34dd9bba(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a specified Unicode character into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, char)")]
	public extern static System.Text.StringBuilder _d09b2a26b288fbd7(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, char[])")]
	public extern static System.Text.StringBuilder _a4c62411da366ab0(System.Text.StringBuilder instance, Number index, object value);

	///<summary>Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, char[], int, int)")]
	public extern static System.Text.StringBuilder _f5ea58b7b0201715(System.Text.StringBuilder instance, Number index, object value, Number startIndex, Number charCount);

	///<summary>Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, int)")]
	public extern static System.Text.StringBuilder _762de3335798fa24(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a 64-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, long)")]
	public extern static System.Text.StringBuilder _057e461451fbc2f6(System.Text.StringBuilder instance, Number index, BigInt value);

	///<summary>Inserts the string representation of a single-precision floating point number into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, float)")]
	public extern static System.Text.StringBuilder _5fa422ae348735cc(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a double-precision floating-point number into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, double)")]
	public extern static System.Text.StringBuilder _7e09aba586586854(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a decimal number into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, decimal)")]
	public extern static System.Text.StringBuilder _7244d40cd7bdaa7a(System.Text.StringBuilder instance, Number index, string value);

	///<summary>Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, ushort)")]
	public extern static System.Text.StringBuilder _62b03548ac3a7f3c(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, uint)")]
	public extern static System.Text.StringBuilder _865132ea357402b6(System.Text.StringBuilder instance, Number index, Number value);

	///<summary>Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, ulong)")]
	public extern static System.Text.StringBuilder _e98da0d88b51734a(System.Text.StringBuilder instance, Number index, BigInt value);

	///<summary>Inserts the string representation of an object into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, object)")]
	public extern static System.Text.StringBuilder _463fe06f693b73f1(System.Text.StringBuilder instance, Number index, object? value);

	///<summary>Inserts the sequence of characters into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)")]
	public extern static System.Text.StringBuilder _ed1b69fd4bc25279(System.Text.StringBuilder instance, Number index, Uint32Array value);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(string, object)")]
	public extern static System.Text.StringBuilder _77a7606b3d9eca3e(System.Text.StringBuilder instance, string format, object? arg0);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(string, object, object)")]
	public extern static System.Text.StringBuilder _e3954878ec607794(System.Text.StringBuilder instance, string format, object? arg0, object? arg1);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(string, object, object, object)")]
	public extern static System.Text.StringBuilder _5ba4a5dce6c59d24(System.Text.StringBuilder instance, string format, object? arg0, object? arg1, object? arg2);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(string, params object[])")]
	public extern static System.Text.StringBuilder _6fc54e5431a32faa(System.Text.StringBuilder instance, string format,  object args);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(string, params System.ReadOnlySpan<object>)")]
	public extern static System.Text.StringBuilder _79714193eef28be4(System.Text.StringBuilder instance, string format,  object args);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object)")]
	public extern static System.Text.StringBuilder _d2a6136c3496706f(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, object? arg0);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object)")]
	public extern static System.Text.StringBuilder _46fad2ab5d282d81(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, object? arg0, object? arg1);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object, object)")]
	public extern static System.Text.StringBuilder _1b411bcc9ec45bf7(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, object? arg0, object? arg1, object? arg2);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params object[])")]
	public extern static System.Text.StringBuilder _7b93ea5668c90df3(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format,  object args);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params System.ReadOnlySpan<object>)")]
	public extern static System.Text.StringBuilder _99e92b2a2bb0066c(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format,  object args);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)")]
	public extern static System.Text.StringBuilder _c50a53c322d59bfc<TArg0>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object format, object arg0);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)")]
	public extern static System.Text.StringBuilder _529a8de0ce89f30f<TArg0, TArg1>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object format, object arg0, object arg1);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)")]
	public extern static System.Text.StringBuilder _e637f9f49752d183<TArg0, TArg1, TArg2>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object format, object arg0, object arg1, object arg2);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params object[])")]
	public extern static System.Text.StringBuilder _353eb0f30e59595f(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object format,  object args);

	///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)")]
	public extern static System.Text.StringBuilder _c17e25151f610256(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object format,  object args);

	///<summary>Replaces all occurrences of a specified string in this instance with another specified string.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(string, string)")]
	public extern static System.Text.StringBuilder _e11a2e954631c69a(System.Text.StringBuilder instance, string oldValue, string? newValue);

	///<summary>Replaces all instances of one read-only character span with another in this builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public extern static System.Text.StringBuilder _c7be232bff90ab62(System.Text.StringBuilder instance, Uint32Array oldValue, Uint32Array newValue);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Equals(System.Text.StringBuilder)")]
	public extern static bool _843038bb92e97c63(System.Text.StringBuilder instance, object sb);

	///<summary>Returns a value indicating whether the characters in this instance are equal to the characters in a specified read-only character span.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)")]
	public extern static bool _251b340a59afa04d(System.Text.StringBuilder instance, Uint32Array span);

	///<summary>Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(string, string, int, int)")]
	public extern static System.Text.StringBuilder _34859fdec187084f(System.Text.StringBuilder instance, string oldValue, string? newValue, Number startIndex, Number count);

	///<summary>Replaces all instances of one read-only character span with another in part of this builder.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)")]
	public extern static System.Text.StringBuilder _5681048ad18a4b3f(System.Text.StringBuilder instance, Uint32Array oldValue, Uint32Array newValue, Number startIndex, Number count);

	///<summary>Replaces all occurrences of a specified character in this instance with another specified character.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(char, char)")]
	public extern static System.Text.StringBuilder _618d386adc69ad32(System.Text.StringBuilder instance, Number oldChar, Number newChar);

	///<summary>Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Replace(char, char, int, int)")]
	public extern static System.Text.StringBuilder _b1fd321da487f718(System.Text.StringBuilder instance, Number oldChar, Number newChar, Number startIndex, Number count);
}
