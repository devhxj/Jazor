using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Text.StringBuilder")]
public static class StringBuilderModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class.</summary>    [WhiteList("System.Text.StringBuilder.StringBuilder()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew();

    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified capacity.</summary>
    ///<param name="capacity">The suggested starting size of this instance.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.</exception>    [WhiteList("System.Text.StringBuilder.StringBuilder(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew2(Number capacity);

    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string.</summary>
    ///<param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>    [WhiteList("System.Text.StringBuilder.StringBuilder(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew3(string? value);

    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class using the specified string and capacity.</summary>
    ///<param name="value">The string used to initialize the value of the instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>
    ///<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.</exception>    [WhiteList("System.Text.StringBuilder.StringBuilder(string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew4(string? value, Number capacity);

    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class from the specified substring and capacity.</summary>
    ///<param name="value">The string that contains the substring used to initialize the value of this instance. If <paramref name="value" /> is <see langword="null" />, the new <see cref="T:System.Text.StringBuilder" /> will contain the empty string (that is, it contains <see cref="F:System.String.Empty" />).</param>
    ///<param name="startIndex">The position within <paramref name="value" /> where the substring begins.</param>
    ///<param name="length">The number of characters in the substring.</param>
    ///<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero. -or- <paramref name="startIndex" /> plus <paramref name="length" /> is not a position within <paramref name="value" />.</exception>    [WhiteList("System.Text.StringBuilder.StringBuilder(string, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew5(string? value, Number startIndex, Number length, Number capacity);

    ///<summary>Initializes a new instance of the <see cref="T:System.Text.StringBuilder" /> class that starts with a specified capacity and can grow to a specified maximum.</summary>
    ///<param name="capacity">The suggested starting size of the <see cref="T:System.Text.StringBuilder" />.</param>
    ///<param name="maxCapacity">The maximum number of characters the current string can contain.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="maxCapacity" /> is less than one, <paramref name="capacity" /> is less than zero, or <paramref name="capacity" /> is greater than <paramref name="maxCapacity" />.</exception>    [WhiteList("System.Text.StringBuilder.StringBuilder(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderNew6(Number capacity, Number maxCapacity);

    [WhiteList("System.Text.StringBuilder.Capacity.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringBuilderGetCapacity(System.Text.StringBuilder instance);

    [WhiteList("System.Text.StringBuilder.Capacity.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringBuilderSetCapacity(System.Text.StringBuilder instance, Number value);

    [WhiteList("System.Text.StringBuilder.MaxCapacity.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringBuilderGetMaxCapacity(System.Text.StringBuilder instance);

    ///<summary>Ensures that the capacity of this instance of <see cref="T:System.Text.StringBuilder" /> is at least the specified value.</summary>
    ///<param name="capacity">The minimum capacity to ensure.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>The new capacity of this instance.</returns>    [WhiteList("System.Text.StringBuilder.EnsureCapacity(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringBuilderEnsureCapacity(System.Text.StringBuilder instance, Number capacity);

    ///<summary>Converts the value of this instance to a <see cref="T:System.String" />.</summary>
    ///<returns>A string whose value is the same as this instance.</returns>    [WhiteList("override System.Text.StringBuilder.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringBuilderToString(System.Text.StringBuilder instance);

    ///<summary>Converts the value of a substring of this instance to a <see cref="T:System.String" />.</summary>
    ///<param name="startIndex">The starting position of the substring in this instance.</param>
    ///<param name="length">The length of the substring.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or- The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the length of the current instance.</exception>
    ///<returns>A string whose value is the same as the specified substring of this instance.</returns>    [WhiteList("System.Text.StringBuilder.ToString(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string StringBuilderToString2(System.Text.StringBuilder instance, Number startIndex, Number length);

    ///<summary>Removes all characters from the current <see cref="T:System.Text.StringBuilder" /> instance.</summary>
    ///<returns>An object whose <see cref="P:System.Text.StringBuilder.Length" /> is 0 (zero).</returns>    [WhiteList("System.Text.StringBuilder.Clear()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderClear(System.Text.StringBuilder instance);

    [WhiteList("System.Text.StringBuilder.Length.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringBuilderGetLength(System.Text.StringBuilder instance);

    [WhiteList("System.Text.StringBuilder.Length.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringBuilderSetLength(System.Text.StringBuilder instance, Number value);

    [WhiteList("System.Text.StringBuilder.this[int].get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number StringBuilderThis(System.Text.StringBuilder instance, Number index);

    [WhiteList("System.Text.StringBuilder.this[int].set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringBuilderThis(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Returns an object that can be used to iterate through the chunks of characters represented in a <see langword="ReadOnlyMemory&lt;Char&gt;" /> created from this <see cref="T:System.Text.StringBuilder" /> instance.</summary>
    ///<returns>An enumerator for the chunks in the <see langword="ReadOnlyMemory&lt;Char&gt;" />.</returns>    [WhiteList("System.Text.StringBuilder.GetChunks()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder.ChunkEnumerator StringBuilderGetChunks(System.Text.StringBuilder instance);

    ///<summary>Appends a specified number of copies of the string representation of a Unicode character to this instance.</summary>
    ///<param name="value">The character to append.</param>
    ///<param name="repeatCount">The number of times to append <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="repeatCount" /> is less than zero. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(char, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend(System.Text.StringBuilder instance, Number value, Number repeatCount);

    ///<summary>Appends the string representation of a specified subarray of Unicode characters to this instance.</summary>
    ///<param name="value">A character array.</param>
    ///<param name="startIndex">The starting position in <paramref name="value" />.</param>
    ///<param name="charCount">The number of characters to append.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="charCount" /> is less than zero. -or- <paramref name="startIndex" /> is less than zero. -or- <paramref name="startIndex" /> + <paramref name="charCount" /> is greater than the length of <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend2(System.Text.StringBuilder instance, char[]? value, Number startIndex, Number charCount);

    ///<summary>Appends a copy of the specified string to this instance.</summary>
    ///<param name="value">The string to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend3(System.Text.StringBuilder instance, string? value);

    ///<summary>Appends a copy of a specified substring to this instance.</summary>
    ///<param name="value">The string that contains the substring to append.</param>
    ///<param name="startIndex">The starting position of the substring within <paramref name="value" />.</param>
    ///<param name="count">The number of characters in <paramref name="value" /> to append.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="count" /> are not zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="count" /> less than zero. -or- <paramref name="startIndex" /> less than zero. -or- <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend4(System.Text.StringBuilder instance, string? value, Number startIndex, Number count);

    ///<summary>Appends the string representation of a specified string builder to this instance.</summary>
    ///<param name="value">The string builder to append.</param>
    ///<returns>A reference to this instance after the append operation is completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(System.Text.StringBuilder)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend5(System.Text.StringBuilder instance, System.Text.StringBuilder? value);

    ///<summary>Appends a copy of a substring within a specified string builder to this instance.</summary>
    ///<param name="value">The string builder that contains the substring to append.</param>
    ///<param name="startIndex">The starting position of the substring within <paramref name="value" />.</param>
    ///<param name="count">The number of characters in <paramref name="value" /> to append.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(System.Text.StringBuilder, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend6(System.Text.StringBuilder instance, System.Text.StringBuilder? value, Number startIndex, Number count);

    ///<summary>Appends the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendLine()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendLine(System.Text.StringBuilder instance);

    ///<summary>Appends a copy of the specified string followed by the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
    ///<param name="value">The string to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendLine(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendLine2(System.Text.StringBuilder instance, string? value);

    ///<summary>Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.</summary>
    ///<param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
    ///<param name="destination">The array where characters will be copied.</param>
    ///<param name="destinationIndex">The starting position in <paramref name="destination" /> where characters will be copied. The index is zero-based.</param>
    ///<param name="count">The number of characters to be copied.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="destination" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" />, is less than zero. -or- <paramref name="sourceIndex" /> is greater than the length of this instance.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="sourceIndex" /> + <paramref name="count" /> is greater than the length of this instance. -or- <paramref name="destinationIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="destination" />.</exception>    [WhiteList("System.Text.StringBuilder.CopyTo(int, char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringBuilderCopyTo(System.Text.StringBuilder instance, Number sourceIndex, char[] destination, Number destinationIndex, Number count);

    ///<summary>Copies the characters from a specified segment of this instance to a destination <see cref="T:System.Char" /> span.</summary>
    ///<param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
    ///<param name="destination">The writable span where characters will be copied.</param>
    ///<param name="count">The number of characters to be copied.</param>    [WhiteList("System.Text.StringBuilder.CopyTo(int, System.Span<char>, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void StringBuilderCopyTo2(System.Text.StringBuilder instance, Number sourceIndex, Uint32Array destination, Number count);

    ///<summary>Inserts one or more copies of a specified string into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The string to insert.</param>
    ///<param name="count">The number of times to insert <paramref name="value" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the current length of this instance. -or- <paramref name="count" /> is less than zero.</exception>
    ///<exception cref="T:System.OutOfMemoryException">The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> times <paramref name="count" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after insertion has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, string, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert(System.Text.StringBuilder instance, Number index, string? value, Number count);

    ///<summary>Removes the specified range of characters from this instance.</summary>
    ///<param name="startIndex">The zero-based position in this instance where removal begins.</param>
    ///<param name="length">The number of characters to remove.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">If <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, or <paramref name="startIndex" /> + <paramref name="length" /> is greater than the length of this instance.</exception>
    ///<returns>A reference to this instance after the excise operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Remove(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderRemove(System.Text.StringBuilder instance, Number startIndex, Number length);

    ///<summary>Appends the string representation of a specified Boolean value to this instance.</summary>
    ///<param name="value">The Boolean value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend7(System.Text.StringBuilder instance, bool value);

    ///<summary>Appends the string representation of a specified <see cref="T:System.Char" /> object to this instance.</summary>
    ///<param name="value">The UTF-16-encoded code unit to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend8(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 8-bit signed integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(sbyte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend9(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 8-bit unsigned integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend10(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 16-bit signed integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend11(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 32-bit signed integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend12(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 64-bit signed integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend13(System.Text.StringBuilder instance, BigInt value);

    ///<summary>Appends the string representation of a specified single-precision floating-point number to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend14(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified double-precision floating-point number to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend15(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified decimal number to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend16(System.Text.StringBuilder instance, String value);

    ///<summary>Appends the string representation of a specified 16-bit unsigned integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(ushort)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend17(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 32-bit unsigned integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend18(System.Text.StringBuilder instance, Number value);

    ///<summary>Appends the string representation of a specified 64-bit unsigned integer to this instance.</summary>
    ///<param name="value">The value to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(ulong)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend19(System.Text.StringBuilder instance, BigInt value);

    ///<summary>Appends the string representation of a specified object to this instance.</summary>
    ///<param name="value">The object to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend20(System.Text.StringBuilder instance, Object? value);

    ///<summary>Appends the string representation of the Unicode characters in a specified array to this instance.</summary>
    ///<param name="value">The array of characters to append.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend21(System.Text.StringBuilder instance, char[]? value);

    ///<summary>Appends the string representation of a specified read-only character span to this instance.</summary>
    ///<param name="value">The read-only character span to append.</param>
    ///<returns>A reference to this instance after the append operation is completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend22(System.Text.StringBuilder instance, Uint32Array value);

    ///<summary>Appends the string representation of a specified read-only character memory region to this instance.</summary>
    ///<param name="value">The read-only character memory region to append.</param>
    ///<returns>A reference to this instance after the append operation is completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(System.ReadOnlyMemory<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend23(System.Text.StringBuilder instance, System.ReadOnlyMemory<char> value);

    ///<summary>Appends the specified interpolated string to this instance.</summary>
    ///<param name="handler">The interpolated string to append.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend24(System.Text.StringBuilder instance, RefValue<System.Text.StringBuilder.AppendInterpolatedStringHandler> handler);

    ///<summary>Appends the specified interpolated string to this instance using the specified format.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="handler">The interpolated string to append.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend25(System.Text.StringBuilder instance, Intl.NumberFormat? provider, RefValue<System.Text.StringBuilder.AppendInterpolatedStringHandler> handler);

    ///<summary>Appends the specified interpolated string followed by the default line terminator to the end of the current StringBuilder object.</summary>
    ///<param name="handler">The interpolated string to append.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendLine(ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendLine3(System.Text.StringBuilder instance, RefValue<System.Text.StringBuilder.AppendInterpolatedStringHandler> handler);

    ///<summary>Appends the specified interpolated string using the specified format, followed by the default line terminator, to the end of the current StringBuilder object.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="handler">The interpolated string to append.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendLine(System.IFormatProvider, ref System.Text.StringBuilder.AppendInterpolatedStringHandler)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendLine4(System.Text.StringBuilder instance, Intl.NumberFormat? provider, RefValue<System.Text.StringBuilder.AppendInterpolatedStringHandler> handler);

    ///<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin(System.Text.StringBuilder instance, string? separator, params object?[] values);

    ///<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified separator between each member, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin2(System.Text.StringBuilder instance, string? separator, params System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates and appends the members of a collection, using the specified separator between each member.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the concatenated and appended strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin<T>(string, System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin3<T>(System.Text.StringBuilder instance, string? separator, IEnumerable<T> values);

    ///<summary>Concatenates the strings of the provided array, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(string, params string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin4(System.Text.StringBuilder instance, string? separator, params string?[] values);

    ///<summary>Concatenates the strings of the provided span, using the specified separator between each string, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(string, params System.ReadOnlySpan<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin5(System.Text.StringBuilder instance, string? separator, params System.ReadOnlySpan<string?> values);

    ///<summary>Concatenates the string representations of the elements in the provided array of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(char, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin6(System.Text.StringBuilder instance, Number separator, params object?[] values);

    ///<summary>Concatenates the string representations of the elements in the provided span of objects, using the specified char separator between each member, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin7(System.Text.StringBuilder instance, Number separator, params System.ReadOnlySpan<object?> values);

    ///<summary>Concatenates and appends the members of a collection, using the specified char separator between each member.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the concatenated and appended strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
    ///<typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin<T>(char, System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin8<T>(System.Text.StringBuilder instance, Number separator, IEnumerable<T> values);

    ///<summary>Concatenates the strings of the provided array, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(char, params string[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin9(System.Text.StringBuilder instance, Number separator, params string?[] values);

    ///<summary>Concatenates the strings of the provided span, using the specified char separator between each string, then appends the result to the current instance of the string builder.</summary>
    ///<param name="separator">The character to use as a separator. <paramref name="separator" /> is included in the joined strings only if <paramref name="values" /> has more than one element.</param>
    ///<param name="values">A span that contains the strings to concatenate and append to the current instance of the string builder.</param>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendJoin(char, params System.ReadOnlySpan<string>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendJoin10(System.Text.StringBuilder instance, Number separator, params System.ReadOnlySpan<string?> values);

    ///<summary>Inserts a string into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The string to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the current length of this instance. -or- The current length of this <see cref="T:System.Text.StringBuilder" /> object plus the length of <paramref name="value" /> exceeds <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert2(System.Text.StringBuilder instance, Number index, string? value);

    ///<summary>Inserts the string representation of a Boolean value into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert3(System.Text.StringBuilder instance, Number index, bool value);

    ///<summary>Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, sbyte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert4(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert5(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, short)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert6(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a specified Unicode character into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert7(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The character array to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, char[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert8(System.Text.StringBuilder instance, Number index, char[]? value);

    ///<summary>Inserts the string representation of a specified subarray of Unicode characters into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">A character array.</param>
    ///<param name="startIndex">The starting index within <paramref name="value" />.</param>
    ///<param name="charCount">The number of characters to insert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="value" /> is <see langword="null" />, and <paramref name="startIndex" /> and <paramref name="charCount" /> are not zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" />, <paramref name="startIndex" />, or <paramref name="charCount" /> is less than zero. -or- <paramref name="index" /> is greater than the length of this instance. -or- <paramref name="startIndex" /> plus <paramref name="charCount" /> is not a position within <paramref name="value" />. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, char[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert9(System.Text.StringBuilder instance, Number index, char[]? value, Number startIndex, Number charCount);

    ///<summary>Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert10(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a 64-bit signed integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert11(System.Text.StringBuilder instance, Number index, BigInt value);

    ///<summary>Inserts the string representation of a single-precision floating point number into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, float)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert12(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a double-precision floating-point number into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert13(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a decimal number into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, decimal)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert14(System.Text.StringBuilder instance, Number index, String value);

    ///<summary>Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, ushort)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert15(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, uint)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert16(System.Text.StringBuilder instance, Number index, Number value);

    ///<summary>Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The value to insert.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, ulong)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert17(System.Text.StringBuilder instance, Number index, BigInt value);

    ///<summary>Inserts the string representation of an object into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The object to insert, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the length of this instance.</exception>
    ///<exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert18(System.Text.StringBuilder instance, Number index, Object? value);

    ///<summary>Inserts the sequence of characters into this instance at the specified character position.</summary>
    ///<param name="index">The position in this instance where insertion begins.</param>
    ///<param name="value">The character span to insert.</param>
    ///<returns>A reference to this instance after the insert operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Insert(int, System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderInsert19(System.Text.StringBuilder instance, Number index, Uint32Array value);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">An object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 1.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of <paramref name="arg0" />.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(string, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat(System.Text.StringBuilder instance, string format, Object? arg0);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 2.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(string, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat2(System.Text.StringBuilder instance, string format, Object? arg0, Object? arg1);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 3.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(string, object, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat3(System.Text.StringBuilder instance, string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An array of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat4(System.Text.StringBuilder instance, string format, params object?[] args);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span.</summary>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">A span of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> span.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat5(System.Text.StringBuilder instance, string format, params System.ReadOnlySpan<object?> args);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to one (1).</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> in which any format specification is replaced by the string representation of <paramref name="arg0" />.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat6(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, Object? arg0);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 2 (two).</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat7(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to 3 (three).</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, object, object, object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat8(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, Object? arg0, Object? arg1, Object? arg2);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">An array of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid. -or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat9(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, params object?[] args);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter span using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A composite format string.</param>
    ///<param name="args">A span of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> span.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat10(System.Text.StringBuilder instance, Intl.NumberFormat? provider, string format, params System.ReadOnlySpan<object?> args);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat<TArg0>(System.IFormatProvider, System.Text.CompositeFormat, TArg0)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat11<TArg0>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<typeparam name="TArg1">The type of the second object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat<TArg0, TArg1>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat12<TArg0, TArg1>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="arg0">The first object to format.</param>
    ///<param name="arg1">The second object to format.</param>
    ///<param name="arg2">The third object to format.</param>
    ///<typeparam name="TArg0">The type of the first object to format.</typeparam>
    ///<typeparam name="TArg1">The type of the second object to format.</typeparam>
    ///<typeparam name="TArg2">The type of the third object to format.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat<TArg0, TArg1, TArg2>(System.IFormatProvider, System.Text.CompositeFormat, TArg0, TArg1, TArg2)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat13<TArg0, TArg1, TArg2>(System.Text.StringBuilder instance, Intl.NumberFormat? provider, System.Text.CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">An array of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> or  <paramref name="args" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params object[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat14(System.Text.StringBuilder instance, Intl.NumberFormat? provider, System.Text.CompositeFormat format, params object?[] args);

    ///<summary>Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.            Each format item is replaced by the string representation of any of the arguments using a specified format provider.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<param name="format">A <see cref="T:System.Text.CompositeFormat" />.</param>
    ///<param name="args">A span of objects to format.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="format" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.AppendFormat(System.IFormatProvider, System.Text.CompositeFormat, params System.ReadOnlySpan<object>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppendFormat15(System.Text.StringBuilder instance, Intl.NumberFormat? provider, System.Text.CompositeFormat format, params System.ReadOnlySpan<object?> args);

    ///<summary>Replaces all occurrences of a specified string in this instance with another specified string.</summary>
    ///<param name="oldValue">The string to replace.</param>
    ///<param name="newValue">The string that replaces <paramref name="oldValue" />, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" />.</returns>    [WhiteList("System.Text.StringBuilder.Replace(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace(System.Text.StringBuilder instance, string oldValue, string? newValue);

    ///<summary>Replaces all instances of one read-only character span with another in this builder.</summary>
    ///<param name="oldValue">The read-only character span to replace.</param>
    ///<param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
    ///<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>    [WhiteList("System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace2(System.Text.StringBuilder instance, Uint32Array oldValue, Uint32Array newValue);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="sb">An object to compare with this instance, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if this instance and <paramref name="sb" /> have equal string, <see cref="P:System.Text.StringBuilder.Capacity" />, and <see cref="P:System.Text.StringBuilder.MaxCapacity" /> values; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Text.StringBuilder.Equals(System.Text.StringBuilder)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringBuilderEquals(System.Text.StringBuilder instance, System.Text.StringBuilder? sb);

    ///<summary>Returns a value indicating whether the characters in this instance are equal to the characters in a specified read-only character span.</summary>
    ///<param name="span">The character span to compare with the current instance.</param>
    ///<returns>  <see langword="true" /> if the characters in this instance and <paramref name="span" /> are the same; otherwise, <see langword="false" />.</returns>    [WhiteList("System.Text.StringBuilder.Equals(System.ReadOnlySpan<char>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool StringBuilderEquals2(System.Text.StringBuilder instance, Uint32Array span);

    ///<summary>Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.</summary>
    ///<param name="oldValue">The string to replace.</param>
    ///<param name="newValue">The string that replaces <paramref name="oldValue" />, or <see langword="null" />.</param>
    ///<param name="startIndex">The position in this instance where the substring begins.</param>
    ///<param name="count">The length of the substring.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="oldValue" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.ArgumentException">The length of <paramref name="oldValue" /> is zero.</exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> or <paramref name="count" /> is less than zero. -or- <paramref name="startIndex" /> plus <paramref name="count" /> indicates a character position not within this instance. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<returns>A reference to this instance with all instances of <paramref name="oldValue" /> replaced by <paramref name="newValue" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> - 1.</returns>    [WhiteList("System.Text.StringBuilder.Replace(string, string, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace3(System.Text.StringBuilder instance, string oldValue, string? newValue, Number startIndex, Number count);

    ///<summary>Replaces all instances of one read-only character span with another in part of this builder.</summary>
    ///<param name="oldValue">The read-only character span to replace.</param>
    ///<param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
    ///<param name="startIndex">The index to start in this builder.</param>
    ///<param name="count">The number of characters to read in this builder.</param>
    ///<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>    [WhiteList("System.Text.StringBuilder.Replace(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace4(System.Text.StringBuilder instance, Uint32Array oldValue, Uint32Array newValue, Number startIndex, Number count);

    ///<summary>Replaces all occurrences of a specified character in this instance with another specified character.</summary>
    ///<param name="oldChar">The character to replace.</param>
    ///<param name="newChar">The character that replaces <paramref name="oldChar" />.</param>
    ///<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" />.</returns>    [WhiteList("System.Text.StringBuilder.Replace(char, char)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace5(System.Text.StringBuilder instance, Number oldChar, Number newChar);

    ///<summary>Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.</summary>
    ///<param name="oldChar">The character to replace.</param>
    ///<param name="newChar">The character that replaces <paramref name="oldChar" />.</param>
    ///<param name="startIndex">The position in this instance where the substring begins.</param>
    ///<param name="count">The length of the substring.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of the value of this instance. -or- <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.</exception>
    ///<returns>A reference to this instance with <paramref name="oldChar" /> replaced by <paramref name="newChar" /> in the range from <paramref name="startIndex" /> to <paramref name="startIndex" /> + <paramref name="count" /> -1.</returns>    [WhiteList("System.Text.StringBuilder.Replace(char, char, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderReplace6(System.Text.StringBuilder instance, Number oldChar, Number newChar, Number startIndex, Number count);

    ///<summary>Appends an array of Unicode characters starting at a specified address to this instance.</summary>
    ///<param name="value">A pointer to an array of characters.</param>
    ///<param name="valueCount">The number of characters in the array.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="valueCount" /> is less than zero. -or- Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity" />.</exception>
    ///<exception cref="T:System.NullReferenceException">  <paramref name="value" /> is a null pointer.</exception>
    ///<returns>A reference to this instance after the append operation has completed.</returns>    [WhiteList("System.Text.StringBuilder.Append(char*, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Text.StringBuilder StringBuilderAppend26(System.Text.StringBuilder instance, char* value, Number valueCount);
}
