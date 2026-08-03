namespace Jazor.CLR;

[ECMAScriptModule("System/Text/StringBuilderModule.js")]
[Jazor(Op.Alias, "System.Text.StringBuilder","String")]
public static class StringBuilderModule
{
	private static void EnsureInstance(Array<string> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	private static void EnsureNonNegative(Number value, string parameterName)
	{
		EnsureWholeNumber(value, parameterName);
		if (value < 0)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} cannot be negative.");
	}

	private static void EnsureInsertIndex(Array<string> instance, Number index)
	{
		EnsureInstance(instance);
		EnsureWholeNumber(index, "index");
		if (index < 0 || index > instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is outside the builder.");
	}

	private static void EnsureExistingIndex(Array<string> instance, Number index)
	{
		EnsureInstance(instance);
		EnsureWholeNumber(index, "index");
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is outside the builder.");
	}

	private static void EnsureRange(Array<string> instance, Number startIndex, Number length)
	{
		EnsureInstance(instance);
		EnsureNonNegative(startIndex, "startIndex");
		EnsureNonNegative(length, "length");
		if (startIndex > instance.Length - length)
			throw new Error("ArgumentOutOfRangeException: startIndex and length must identify a valid range.");
	}

	private static string JoinRange(Array<string> value, Number startIndex, Number length)
	{
		var result = "";
		for (var offset = 0; offset < length; offset++)
			result += value[startIndex + offset];
		return result;
	}

	private static Array<string> AppendText(Array<string> instance, string? value)
	{
		EnsureInstance(instance);
		if (value == null)
			return instance;

		for (var index = 0; index < value.Length; index++)
			instance.Push(value[index].ToString());
		return instance;
	}

	private static Array<string> AppendJoinedStrings(
		Array<string> instance,
		string? separator,
		Array<string?> values)
	{
		EnsureInstance(instance);
		if (values == null)
			throw new Error("ArgumentNullException: values is null.");

		// Build the text before mutation. This keeps enumeration snapshot semantics even if
		// host-side carrier reuse ever makes values alias the builder's backing array.
		var text = "";
		for (var index = 0; index < values.Length; index++)
		{
			if (index != 0)
				text += separator ?? "";
			text += values[index] ?? "";
		}

		return AppendText(instance, text);
	}

	private static Array<string> AppendArrayRange(
		Array<string> instance,
		Array<string>? value,
		Number startIndex,
		Number count)
	{
		EnsureInstance(instance);
		EnsureNonNegative(startIndex, "startIndex");
		EnsureNonNegative(count, "count");
		if (value == null)
		{
			if (startIndex == 0 && count == 0)
				return instance;
			throw new Error("ArgumentNullException: value is null.");
		}
		if (startIndex > value.Length - count)
			throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");

		// Capture the range before mutation so self-append keeps StringBuilder snapshot semantics.
		var snapshot = value.Slice(startIndex, startIndex + count);
		for (var index = 0; index < snapshot.Length; index++)
			instance.Push(snapshot[index]);
		return instance;
	}

	private static Array<string> AppendStringRange(
		Array<string> instance,
		string? value,
		Number startIndex,
		Number count)
	{
		EnsureInstance(instance);
		EnsureNonNegative(startIndex, "startIndex");
		EnsureNonNegative(count, "count");
		if (value == null)
		{
			if (startIndex == 0 && count == 0)
				return instance;
			throw new Error("ArgumentNullException: value is null.");
		}
		if (startIndex > value.Length - count)
			throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");

		for (var offset = 0; offset < count; offset++)
			instance.Push(value[startIndex + offset].ToString());
		return instance;
	}

	private static Array<string> InsertText(Array<string> instance, Number index, string? value, Number count)
	{
		EnsureInsertIndex(instance, index);
		EnsureNonNegative(count, "count");
		if (value == null || value.Length == 0 || count == 0)
			return instance;

		// Reverse insertion preserves text order while using one stable array mutation primitive.
		for (var repeat = 0; repeat < count; repeat++)
		{
			for (var offset = value.Length - 1; offset >= 0; offset--)
				instance.Splice(index, 0, value[offset].ToString());
		}
		return instance;
	}

	private static Array<string> InsertArrayRange(
		Array<string> instance,
		Number index,
		Array<string>? value,
		Number startIndex,
		Number count)
	{
		EnsureInsertIndex(instance, index);
		EnsureNonNegative(startIndex, "startIndex");
		EnsureNonNegative(count, "charCount");
		if (value == null)
		{
			if (startIndex == 0 && count == 0)
				return instance;
			throw new Error("ArgumentNullException: value is null.");
		}
		if (startIndex > value.Length - count)
			throw new Error("ArgumentOutOfRangeException: startIndex and charCount must identify a valid range.");

		return InsertText(instance, index, JoinRange(value, startIndex, count), 1);
	}

	private static Array<string> ReplaceTextRange(
		Array<string> instance,
		string oldValue,
		string? newValue,
		Number startIndex,
		Number count)
	{
		if (oldValue == null)
			throw new Error("ArgumentNullException: oldValue is null.");
		if (oldValue.Length == 0)
			throw new Error("ArgumentException: oldValue cannot be empty.");
		EnsureRange(instance, startIndex, count);

		var replaced = JoinRange(instance, startIndex, count).ReplaceAll(oldValue, newValue ?? "");
		instance.Splice(startIndex, count);
		return InsertText(instance, startIndex, replaced, 1);
	}
	/// <summary>
	/// C#: new StringBuilder()
	/// JS: [] (空数组)
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.StringBuilder()", "[]")]
	public extern static System.Text.StringBuilder _2154365d1f9a2abf();

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified capacity.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.StringBuilder(int)")]
	public static Array<string> _404c94878c905b27(Number capacity)
	{
		EnsureNonNegative(capacity, "capacity");
		return new Array<string>();
	}

	/// <summary>
	/// C#: new StringBuilder(string)
	/// JS: (value ?? "").split("")
	/// 保持 UTF-16 code unit 语义，与逐字符循环行为一致。
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.StringBuilder(string)", "(__arg1 ?? '').split('')")]
	public extern static System.Text.StringBuilder _c2c8c4778873ccdc(string? value);

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string and capacity.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.StringBuilder(string, int)")]
	public static Array<string> _8ddc5378f62c27cc(string? value, Number capacity)
	{
		EnsureNonNegative(capacity, "capacity");
		return AppendText(new Array<string>(), value);
	}

	///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class from the specified substring and capacity.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.StringBuilder(string, int, int, int)")]
	public static Array<string> _70c61ab8ef3313c3(string? value, Number startIndex, Number length, Number capacity)
	{
		EnsureNonNegative(capacity, "capacity");
		return AppendStringRange(new Array<string>(), value ?? "", startIndex, length);
	}

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

	/// <summary>
	/// C#: sb.ToString()
	/// JS: instance.join('')
	/// </summary>
	[Jazor(Op.Inline, "override System.Text.StringBuilder.ToString()", "__arg1.join('')")]
	public extern static string _010347a06fe9584c(Array<string> instance);

	///<summary>Converts the value of a substring of this instance to a <see cref="T:System.String" />.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.ToString(int, int)")]
	public static string _4941946dde4f03f0(Array<string> instance, Number startIndex, Number length)
	{
		EnsureRange(instance, startIndex, length);
		return JoinRange(instance, startIndex, length);
	}

	/// <summary>
	/// C#: sb.Clear()
	/// JS: (instance.length = 0, instance)
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.Clear()", "(__arg1.length = 0, __arg1)")]
	public extern static System.Text.StringBuilder _3b8e77fc2c4d5f63(Array<string> instance);

	/// <summary>
	/// C#: sb.Length
	/// JS: instance.length
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.Length.get", "__arg1.length")]
	public extern static Number _76a78d5aa26cb6e0(System.Text.StringBuilder instance);

	[Jazor(Op.Import ,"System.Text.StringBuilder.Length.set")]
	public static void _085925374c6d3abd(Array<string> instance, Number value)
	{
		EnsureInstance(instance);
		EnsureNonNegative(value, "value");
		if (value < instance.Length)
		{
			instance.Splice(value, instance.Length - value);
			return;
		}

		while (instance.Length < value)
			instance.Push("\0");
	}

	[Jazor(Op.Import ,"System.Text.StringBuilder.this[int].get")]
	public static string _c59f10eccb1d75d4(Array<string> instance, Number index)
	{
		EnsureExistingIndex(instance, index);
		return instance[index];
	}

	[Jazor(Op.Import ,"System.Text.StringBuilder.this[int].set")]
	public static void _a970d620cd814959(Array<string> instance, Number index, string value)
	{
		EnsureExistingIndex(instance, index);
		instance[index] = value;
	}

	///<summary>Returns an object that can be used to iterate through the chunks of characters represented in a <see langword="ReadOnlyMemory&lt;Char&gt;" /> created from this <see cref="T:System.Text.StringBuilder" /> instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.GetChunks()")]
	public extern static System.Text.StringBuilder.ChunkEnumerator _eb70112718b443d3(System.Text.StringBuilder instance);

	///<summary>Appends a specified number of copies of the string representation of a Unicode character to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(char, int)")]
	public static Array<string> _77869f53e4b4cf63(Array<string> instance, string value, Number repeatCount)
		=> InsertText(instance, instance.Length, value, repeatCount);

	///<summary>Appends the string representation of a specified subarray of Unicode characters to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(char[], int, int)")]
	public static Array<string> _76a6be47564b1442(Array<string> instance, Array<string>? value, Number startIndex, Number charCount)
		=> AppendArrayRange(instance, value, startIndex, charCount);

	/// <summary>
	/// C#: sb.Append(string)
	/// JS: push(...(value ?? "").split(""))，再返回 instance
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.Append(string)", "(__arg1.push(...(__arg2 ?? '').split('')), __arg1)")]
	public extern static System.Text.StringBuilder _2879b76db56f25fb(Array<string> instance, string? value);

	///<summary>Appends a copy of a specified substring to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(string, int, int)")]
	public static Array<string> _643a38ba616afd42(Array<string> instance, string? value, Number startIndex, Number count)
		=> AppendStringRange(instance, value, startIndex, count);

	///<summary>Appends the string representation of a specified string builder to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(System.Text.StringBuilder)")]
	public static Array<string> _390481e4ef6d1b43(Array<string> instance, Array<string>? value)
		=> AppendArrayRange(instance, value, 0, value?.Length ?? 0);

	///<summary>Appends a copy of a substring within a specified string builder to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)")]
	public static Array<string> _2a75c7a6bec12592(Array<string> instance, Array<string>? value, Number startIndex, Number count)
		=> AppendArrayRange(instance, value, startIndex, count);

	/// <summary>
	/// C#: sb.AppendLine()
	/// JS: push('\n')，再返回 instance
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.AppendLine()", "(__arg1.push('\\n'), __arg1)")]
	public extern static System.Text.StringBuilder _35fe8bcf463e879b(Array<string> instance);

	/// <summary>
	/// C#: sb.AppendLine(string)
	/// JS: 先追加 value，再 push('\n')，最后返回 instance
	/// </summary>
	[Jazor(Op.Inline, "System.Text.StringBuilder.AppendLine(string)", "(__arg1.push(...(__arg2 ?? '').split('')), __arg1.push('\\n'), __arg1)")]
	public extern static System.Text.StringBuilder _c06aaa44e213e405(Array<string> instance, string? value);

	///<summary>Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.CopyTo(int, char[], int, int)")]
	public static void _e7c76d547b84e1dd(
		Array<string> instance,
		Number sourceIndex,
		Array<string> destination,
		Number destinationIndex,
		Number count)
	{
		EnsureInstance(instance);
		EnsureNonNegative(sourceIndex, "sourceIndex");
		if (destination == null)
			throw new Error("ArgumentNullException: destination is null.");
		EnsureNonNegative(destinationIndex, "destinationIndex");
		EnsureNonNegative(count, "count");
		if (sourceIndex > instance.Length - count)
			throw new Error("ArgumentException: source range exceeds the builder.");
		if (destinationIndex > destination.Length - count)
			throw new Error("ArgumentException: destination array is too small.");

		for (var offset = 0; offset < count; offset++)
			destination[destinationIndex + offset] = instance[sourceIndex + offset];
	}

	///<summary>Copies the characters from a specified segment of this instance to a destination <see cref="T:System.Char" /> span.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.CopyTo(int, System.Span<char>, int)")]
	public extern static void _54205e7ac737a01c(System.Text.StringBuilder instance, Number sourceIndex, Uint32Array destination, Number count);

	///<summary>Inserts one or more copies of a specified string into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, string, int)")]
	public static Array<string> _da897479d9bd6139(Array<string> instance, Number index, string? value, Number count)
		=> InsertText(instance, index, value, count);

	///<summary>Removes the specified range of characters from this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Remove(int, int)")]
	public static Array<string> _152bf60dc35a5bb6(Array<string> instance, Number startIndex, Number length)
	{
		EnsureRange(instance, startIndex, length);
		instance.Splice(startIndex, length);
		return instance;
	}

	///<summary>Appends the string representation of a specified Boolean value to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(bool)")]
	public static Array<string> _dded353c61620d12(Array<string> instance, bool value)
		=> AppendText(instance, value ? "True" : "False");

	///<summary>Appends the string representation of a specified <see cref="T:System.Char" /> object to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(char)")]
	public static Array<string> _a2ce7c5adfc1553c(Array<string> instance, string value)
		=> AppendText(instance, value);

	///<summary>Appends the string representation of a specified 8-bit signed integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(sbyte)")]
	public static Array<string> _3ce4c9341fd5777f(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 8-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(byte)")]
	public static Array<string> _d530c416b64aac49(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 16-bit signed integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(short)")]
	public static Array<string> _ea789609ea3aeeb0(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 32-bit signed integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(int)")]
	public static Array<string> _212b9738d2ea3b2d(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 64-bit signed integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(long)")]
	public static Array<string> _a20035534ee530dd(Array<string> instance, BigInt value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified single-precision floating-point number to this instance.</summary>
	// Keep this aligned with the existing float.ToString() carrier contract rather than
	// introducing a second formatter only for StringBuilder.
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(float)")]
	public static Array<string> _ec1b541b6a274b24(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified double-precision floating-point number to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(double)")]
	public static Array<string> _817e46ee3d60bf66(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified decimal number to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(decimal)")]
	public static Array<string> _f07022820ca3881f(Array<string> instance, string value)
		=> AppendText(instance, value);

	///<summary>Appends the string representation of a specified 16-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(ushort)")]
	public static Array<string> _37e94b64bce60492(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 32-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(uint)")]
	public static Array<string> _423a4a09f9fa54c4(Array<string> instance, Number value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified 64-bit unsigned integer to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(ulong)")]
	public static Array<string> _f09314f07502e2a3(Array<string> instance, BigInt value)
		=> AppendText(instance, value.ToString());

	///<summary>Appends the string representation of a specified object to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(object)")]
	public extern static System.Text.StringBuilder _06379efa8addb10d(System.Text.StringBuilder instance, object? value);

	///<summary>Appends the string representation of the Unicode characters in a specified array to this instance.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Append(char[])")]
	public static Array<string> _4ec74831297581ec(Array<string> instance, Array<string>? value)
		=> AppendArrayRange(instance, value, 0, value?.Length ?? 0);

	///<summary>Appends the string representation of a specified read-only character span to this instance.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)")]
	public static Array<string> _8c68c811d3d42bcf(Array<string> instance, RuntimeModule.JReadOnlyCharSpan value)
		=> AppendText(instance, RuntimeModule.MaterializeReadOnlyCharSpan(value));

	///<summary>Appends the string representation of a specified read-only character memory region to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.ReadOnlyMemory<char>)")]
	public extern static System.Text.StringBuilder _19e34431ab825546(System.Text.StringBuilder instance, object value);

	///<summary>Appends the specified interpolated string to this instance.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _b753ce137296837a(System.Text.StringBuilder instance, object handler);

	///<summary>Appends the specified interpolated string to this instance using the specified format.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Append(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _c38a3237ddfa0a19(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object handler);

	///<summary>Appends the specified interpolated string followed by the default line terminator to the end of the current StringBuilder object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _c52ed5039c53253f(System.Text.StringBuilder instance, object handler);

	///<summary>Appends the specified interpolated string using the specified format, followed by the default line terminator, to the end of the current StringBuilder object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.AppendLine(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
	public extern static Array<object?> _0192e43c680249a7(System.Text.StringBuilder instance, Intl.NumberFormat? provider, object handler);

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
	[Jazor(Op.Import, "System.Text.StringBuilder.AppendJoin(string, params string[])")]
	public static Array<string> _6ceea7a4bfd233b6(Array<string> instance, string? separator, Array<string?> values)
		=> AppendJoinedStrings(instance, separator, values);

	///<summary>Concatenates the strings of the provided span, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)")]
	public static Array<string> _035c615b56218700(Array<string> instance, string? separator, Array<string?> values)
		=> AppendJoinedStrings(instance, separator, values);

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
	[Jazor(Op.Import, "System.Text.StringBuilder.AppendJoin(char, params string[])")]
	public static Array<string> _02a3ec9f0e91877f(Array<string> instance, string separator, Array<string?> values)
		=> AppendJoinedStrings(instance, separator, values);

	///<summary>Concatenates the strings of the provided span, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)")]
	public static Array<string> _08c4f86d45c8b851(Array<string> instance, string separator, Array<string?> values)
		=> AppendJoinedStrings(instance, separator, values);

	///<summary>Inserts a string into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, string)")]
	public static Array<string> _40a305d0112c40d9(Array<string> instance, Number index, string? value)
		=> InsertText(instance, index, value, 1);

	///<summary>Inserts the string representation of a Boolean value into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, bool)")]
	public static Array<string> _2e7808d3cd4780e8(Array<string> instance, Number index, bool value)
		=> InsertText(instance, index, value ? "True" : "False", 1);

	///<summary>Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, sbyte)")]
	public static Array<string> _5d866e86d8040d7d(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, byte)")]
	public static Array<string> _a90cbae6c991fb88(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, short)")]
	public static Array<string> _bf04d5cd34dd9bba(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a specified Unicode character into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, char)")]
	public static Array<string> _d09b2a26b288fbd7(Array<string> instance, Number index, string value)
		=> InsertText(instance, index, value, 1);

	///<summary>Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, char[])")]
	public static Array<string> _a4c62411da366ab0(Array<string> instance, Number index, Array<string>? value)
		=> InsertArrayRange(instance, index, value, 0, value?.Length ?? 0);

	///<summary>Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, char[], int, int)")]
	public static Array<string> _f5ea58b7b0201715(Array<string> instance, Number index, Array<string>? value, Number startIndex, Number charCount)
		=> InsertArrayRange(instance, index, value, startIndex, charCount);

	///<summary>Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, int)")]
	public static Array<string> _762de3335798fa24(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a 64-bit signed integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, long)")]
	public static Array<string> _057e461451fbc2f6(Array<string> instance, Number index, BigInt value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a single-precision floating point number into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, float)")]
	public static Array<string> _5fa422ae348735cc(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a double-precision floating-point number into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, double)")]
	public static Array<string> _7e09aba586586854(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a decimal number into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, decimal)")]
	public static Array<string> _7244d40cd7bdaa7a(Array<string> instance, Number index, string value)
		=> InsertText(instance, index, value, 1);

	///<summary>Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, ushort)")]
	public static Array<string> _62b03548ac3a7f3c(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, uint)")]
	public static Array<string> _865132ea357402b6(Array<string> instance, Number index, Number value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Insert(int, ulong)")]
	public static Array<string> _e98da0d88b51734a(Array<string> instance, Number index, BigInt value)
		=> InsertText(instance, index, value.ToString(), 1);

	///<summary>Inserts the string representation of an object into this instance at the specified character position.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Insert(int, object)")]
	public extern static System.Text.StringBuilder _463fe06f693b73f1(System.Text.StringBuilder instance, Number index, object? value);

	///<summary>Inserts the sequence of characters into this instance at the specified character position.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)")]
	public static Array<string> _ed1b69fd4bc25279(Array<string> instance, Number index, RuntimeModule.JReadOnlyCharSpan value)
		=> InsertText(instance, index, RuntimeModule.MaterializeReadOnlyCharSpan(value), 1);

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
	[Jazor(Op.Import ,"System.Text.StringBuilder.Replace(string, string)")]
	public static Array<string> _e11a2e954631c69a(Array<string> instance, string oldValue, string? newValue)
	{
		EnsureInstance(instance);
		return ReplaceTextRange(instance, oldValue, newValue, 0, instance.Length);
	}

	///<summary>Replaces all instances of one read-only character span with another in this builder.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
	public static Array<string> _c7be232bff90ab62(
		Array<string> instance,
		RuntimeModule.JReadOnlyCharSpan oldValue,
		RuntimeModule.JReadOnlyCharSpan newValue)
		=> ReplaceTextRange(
			instance,
			RuntimeModule.MaterializeReadOnlyCharSpan(oldValue),
			RuntimeModule.MaterializeReadOnlyCharSpan(newValue),
			0,
			instance.Length);

	///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
	[Jazor(Op.Discard ,"System.Text.StringBuilder.Equals(System.Text.StringBuilder)")]
	public extern static bool _843038bb92e97c63(System.Text.StringBuilder instance, object sb);

	///<summary>Returns a value indicating whether the characters in this instance are equal to the characters in a specified read-only character span.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)")]
	public static bool _251b340a59afa04d(Array<string> instance, RuntimeModule.JReadOnlyCharSpan span)
	{
		EnsureInstance(instance);
		return JoinRange(instance, 0, instance.Length) == RuntimeModule.MaterializeReadOnlyCharSpan(span);
	}

	///<summary>Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Replace(string, string, int, int)")]
	public static Array<string> _34859fdec187084f(Array<string> instance, string oldValue, string? newValue, Number startIndex, Number count)
		=> ReplaceTextRange(instance, oldValue, newValue, startIndex, count);

	///<summary>Replaces all instances of one read-only character span with another in part of this builder.</summary>
	[Jazor(Op.Import, "System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)")]
	public static Array<string> _5681048ad18a4b3f(
		Array<string> instance,
		RuntimeModule.JReadOnlyCharSpan oldValue,
		RuntimeModule.JReadOnlyCharSpan newValue,
		Number startIndex,
		Number count)
		=> ReplaceTextRange(
			instance,
			RuntimeModule.MaterializeReadOnlyCharSpan(oldValue),
			RuntimeModule.MaterializeReadOnlyCharSpan(newValue),
			startIndex,
			count);

	///<summary>Replaces all occurrences of a specified character in this instance with another specified character.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Replace(char, char)")]
	public static Array<string> _618d386adc69ad32(Array<string> instance, string oldChar, string newChar)
	{
		EnsureInstance(instance);
		return ReplaceTextRange(instance, oldChar, newChar, 0, instance.Length);
	}

	///<summary>Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.</summary>
	[Jazor(Op.Import ,"System.Text.StringBuilder.Replace(char, char, int, int)")]
	public static Array<string> _b1fd321da487f718(Array<string> instance, string oldChar, string newChar, Number startIndex, Number count)
		=> ReplaceTextRange(instance, oldChar, newChar, startIndex, count);
}
