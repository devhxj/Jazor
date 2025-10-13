namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("byte")]
public static class ByteModule
{
	//byte.MaxValue = 255;

	//byte.MinValue = 0;

    [WhiteList("byte.Byte()")]
	public extern static Number ByteNew();

    ///<summary>Compares this instance to a specified object and returns an indication of their relative values.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Byte" />.</exception>
    ///<returns>A signed integer that indicates the relative order of this instance and <paramref name="value" />. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance is less than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance is greater than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
    [WhiteList("byte.CompareTo(object)")]
	[WhiteList("byte.CompareTo(byte)")]
	[ECMAScriptLiteral("{0} === {1} ? 0 :({0} > {1} ? 1 : -1)")]
	public extern static Number ByteCompareTo(Number instance, Object? value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="obj">An object to compare with this instance, or <see langword="null" />.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Byte" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
    [WhiteList("override byte.Equals(object)")]
	[WhiteList("byte.Equals(byte)")]
	[ECMAScriptLiteral("{0} === {1}")]
	public extern static bool ByteEquals(Number instance, Object? obj);

    ///<summary>Returns the hash code for this instance.</summary>
    ///<returns>A hash code for the current <see cref="T:System.Byte" />.</returns>
    [WhiteList("override byte.GetHashCode()")]
	[ECMAScriptLiteral("{0}")]
	public extern static Number ByteGetHashCode(Number instance);

    ///<summary>Converts the string representation of a number to its <see cref="T:System.Byte" /> equivalent.</summary>
    ///<param name="s">A string that contains a number to convert. The string is interpreted using the <see cref="F:System.Globalization.NumberStyles.Integer" /> style.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
    ///<returns>A byte value that is equivalent to the number contained in <paramref name="s" />.</returns>
    [WhiteList("static byte.Parse(string)")]
    public static Number ByteParse(string? s) 
    {
		if (TypeOf(s) != "string")
			throw new Error("Argument must be a string");

		if (string.IsNullOrWhiteSpace(s))
            throw new Error("ArgumentNullException: s cannot be null or empty");

        var trimmed = s.Trim();

        // 更严格的整数格式验证（只允许数字和可选的正负号）
        if (RegExp(@"!/^[+-]?\d+$/").Test(trimmed)) 
            throw new Error("FormatException: Input string was not in a correct format.");

        var number = ParseInt(trimmed, 10);

        // 再次验证转换结果
        if (IsNaN(number) || number.ToString() != trimmed.Replace(RegExp(@"/^\+/"), ""))
            throw new Error("FormatException: Input string was not in a correct format.");

        if (number < 0 || number > 255)
            throw new Error("OverflowException: Value was either too large or too small for an unsigned byte.");
        
        return number;
    }

    ///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Byte" /> equivalent.</summary>
    ///<param name="s">A string that contains a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<returns>A byte value that is equivalent to the number contained in <paramref name="s" />.</returns>
    [WhiteList("static byte.Parse(string, System.Globalization.NumberStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse2(string s, System.Globalization.NumberStyles style);

    ///<summary>Converts the string representation of a number in a specified culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
    ///<param name="s">A string that contains a number to convert. The string is interpreted using the <see cref="F:System.Globalization.NumberStyles.Integer" /> style.</param>
    ///<param name="provider">An object that supplies culture-specific parsing information about <paramref name="s" />. If <paramref name="provider" /> is <see langword="null" />, the thread current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>.</exception>
    ///<returns>A byte value that is equivalent to the number contained in <paramref name="s" />.</returns>
    [WhiteList("static byte.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse3(string s, Intl.NumberFormat? provider);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
    ///<param name="s">A string that contains a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific information about the format of <paramref name="s" />. If <paramref name="provider" /> is <see langword="null" />, the thread current culture is used.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> is not of the correct format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>. -or- <paramref name="s" /> includes non-zero, fractional digits.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<returns>A byte value that is equivalent to the number contained in <paramref name="s" />.</returns>
    [WhiteList("static byte.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse4(string s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent.</summary>
    ///<param name="s">A span containing the characters representing the value to convert.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific information about the format of <paramref name="s" />. If <paramref name="provider" /> is <see langword="null" />, the thread current culture is used.</param>
    ///<returns>A byte value that is equivalent to the number contained in <paramref name="s" />.</returns>
    [WhiteList("static byte.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to convert the string representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string that contains a number to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Byte" /> value equivalent to the number contained in <paramref name="s" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static byte.TryParse(string, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse(string? s, OutValue<Number> result);

    ///<summary>Tries to convert the span representation of a number to its <see cref="T:System.Byte" /> equivalent, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the <see cref="T:System.Byte" /> value equivalent to the number contained in <paramref name="s" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse2(Uint32Array s, OutValue<Number> result);

    ///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its 8-bit unsigned integer equivalent.</summary>
    ///<param name="utf8Text">A span containing the UTF-8 characters representing the number to convert.</param>
    ///<param name="result">When this method returns, contains the 8-bit unsigned integer value equivalent to the number contained in <paramref name="utf8Text" /> if the conversion succeeded, or zero if the conversion failed. This parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="utf8Text" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse3(Uint8Array utf8Text, OutValue<Number> result);

    ///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A string containing a number to convert. The string is interpreted using the style specified by <paramref name="style" />.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. If <paramref name="provider" /> is <see langword="null" />, the thread current culture is used.</param>
    ///<param name="result">When this method returns, contains the 8-bit unsigned integer value equivalent to the number contained in <paramref name="s" /> if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="style" /> is not a <see cref="T:System.Globalization.NumberStyles" /> value. -or- <paramref name="style" /> is not a combination of <see cref="F:System.Globalization.NumberStyles.AllowHexSpecifier" /> and <see cref="F:System.Globalization.NumberStyles.HexNumber" /> values.</exception>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static byte.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse4(string? s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Byte" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
    ///<param name="s">A span containing the characters representing the number to convert. The span is interpreted using the <see cref="F:System.Globalization.NumberStyles.Integer" /> style.</param>
    ///<param name="style">A bitwise combination of enumeration values that indicates the style elements that can be present in <paramref name="s" />. A typical value to specify is <see cref="F:System.Globalization.NumberStyles.Integer" />.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information about <paramref name="s" />. If <paramref name="provider" /> is <see langword="null" />, the thread current culture is used.</param>
    ///<param name="result">When this method returns, contains the 8-bit unsigned integer value equivalent to the number contained in <paramref name="s" /> if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, is not of the correct format, or represents a number less than <see cref="F:System.Byte.MinValue">Byte.MinValue</see> or greater than <see cref="F:System.Byte.MaxValue">Byte.MaxValue</see>. This parameter is passed uninitialized; any value originally supplied in <paramref name="result" /> will be overwritten.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse5(Uint32Array s, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation.</summary>
    ///<returns>The string representation of the value of this object, which consists of a sequence of digits that range from 0 to 9 with no leading zeroes.</returns>
    [WhiteList("override byte.ToString()")]
	public extern static string ByteToString(Number instance);

    ///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format.</summary>
    ///<param name="format">A numeric format string.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> includes an unsupported specifier. Supported format specifiers are listed in the Remarks section.</exception>
    ///<returns>The string representation of the current <see cref="T:System.Byte" /> object, formatted as specified by the <paramref name="format" /> parameter.</returns>
    [WhiteList("byte.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ByteToString2(Number instance, string? format);

    ///<summary>Converts the numeric value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified culture-specific formatting information.</summary>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<returns>The string representation of the value of this object in the format specified by the <paramref name="provider" /> parameter.</returns>
    [WhiteList("byte.ToString(System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ByteToString3(Number instance, Intl.NumberFormat? provider);

    ///<summary>Converts the value of the current <see cref="T:System.Byte" /> object to its equivalent string representation using the specified format and culture-specific formatting information.</summary>
    ///<param name="format">A standard or custom numeric format string.</param>
    ///<param name="provider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.FormatException">  <paramref name="format" /> includes an unsupported specifier. Supported format specifiers are listed in the Remarks section.</exception>
    ///<returns>The string representation of the current <see cref="T:System.Byte" /> object, formatted as specified by the <paramref name="format" /> and <paramref name="provider" /> parameters.</returns>
    [WhiteList("byte.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string ByteToString4(Number instance, string? format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current 8-bit unsigned integer instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
    [WhiteList("byte.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryFormat(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="provider">An optional object that supplies culture-specific formatting information for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("byte.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryFormat2(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

    ///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Byte" />.</summary>
    ///<returns>The enumerated constant, <see cref="F:System.TypeCode.Byte" />.</returns>
    [WhiteList("byte.GetTypeCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.TypeCode ByteGetTypeCode(Number instance);

    ///<summary>Computes the quotient and remainder of two values.</summary>
    ///<param name="left">The value which <code data-dev-comment-type="paramref">right</code> divides.</param>
    ///<param name="right">The value which divides <code data-dev-comment-type="paramref">left</code>.</param>
    ///<returns>The quotient and remainder of <code data-dev-comment-type="paramref">left</code> divided-by <code data-dev-comment-type="paramref">right</code>.</returns>
    [WhiteList("static byte.DivRem(byte, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static (byte Quotient, byte Remainder) ByteDivRem(Number left, Number right);

    ///<summary>Computes the number of leading zeros in a value.</summary>
    ///<param name="value">The value whose leading zeroes are to be counted.</param>
    ///<returns>The number of leading zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static byte.LeadingZeroCount(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteLeadingZeroCount(Number value);

    ///<summary>Computes the number of bits that are set in a value.</summary>
    ///<param name="value">The value whose set bits are to be counted.</param>
    ///<returns>The number of set bits in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static byte.PopCount(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number BytePopCount(Number value);

    ///<summary>Rotates a value left by a given amount.</summary>
    ///<param name="value">The value which is rotated left by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated left.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> left by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
    [WhiteList("static byte.RotateLeft(byte, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteRotateLeft(Number value, Number rotateAmount);

    ///<summary>Rotates a value right by a given amount.</summary>
    ///<param name="value">The value which is rotated right by <code data-dev-comment-type="paramref">rotateAmount</code>.</param>
    ///<param name="rotateAmount">The amount by which <code data-dev-comment-type="paramref">value</code> is rotated right.</param>
    ///<returns>The result of rotating <code data-dev-comment-type="paramref">value</code> right by <code data-dev-comment-type="paramref">rotateAmount</code>.</returns>
    [WhiteList("static byte.RotateRight(byte, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteRotateRight(Number value, Number rotateAmount);

    ///<summary>Computes the number of trailing zeros in a value.</summary>
    ///<param name="value">The value whose trailing zeroes are to be counted.</param>
    ///<returns>The number of trailing zeros in <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static byte.TrailingZeroCount(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteTrailingZeroCount(Number value);

    ///<summary>Determines if a value is a power of two.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is a power of two; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.IsPow2(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteIsPow2(Number value);

    ///<summary>Computes the log2 of a value.</summary>
    ///<param name="value">The value whose log2 is to be computed.</param>
    ///<returns>The log2 of <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static byte.Log2(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteLog2(Number value);

    ///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
    ///<param name="value">The value to clamp.</param>
    ///<param name="min">The inclusive minimum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<param name="max">The inclusive maximum to which <code data-dev-comment-type="paramref">value</code> should clamp.</param>
    ///<returns>The result of clamping <code data-dev-comment-type="paramref">value</code> to the inclusive range of <code data-dev-comment-type="paramref">min</code> and <code data-dev-comment-type="paramref">max</code>.</returns>
    [WhiteList("static byte.Clamp(byte, byte, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteClamp(Number value, Number min, Number max);

    ///<summary>Compares two values to compute which is greater.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is greater than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
    [WhiteList("static byte.Max(byte, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteMax(Number x, Number y);

    ///<summary>Compares two values to compute which is lesser.</summary>
    ///<param name="x">The value to compare with <code data-dev-comment-type="paramref">y</code>.</param>
    ///<param name="y">The value to compare with <code data-dev-comment-type="paramref">x</code>.</param>
    ///<returns>  <code data-dev-comment-type="paramref">x</code> if it is less than <code data-dev-comment-type="paramref">y</code>; otherwise, <code data-dev-comment-type="paramref">y</code>.</returns>
    [WhiteList("static byte.Min(byte, byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteMin(Number x, Number y);

    ///<summary>Computes the sign of a value.</summary>
    ///<param name="value">The value whose sign is to be computed.</param>
    ///<returns>A positive value if <code data-dev-comment-type="paramref">value</code> is positive, <xref data-throw-if-not-resolved="true" uid="System.Numerics.INumberBase`1.Zero"></xref> if <code data-dev-comment-type="paramref">value</code> is zero, and a negative value if <code data-dev-comment-type="paramref">value</code> is negative.</returns>
    [WhiteList("static byte.Sign(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteSign(Number value);

    ///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>.</returns>
    [WhiteList("static byte.CreateChecked<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteCreateChecked<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, saturating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
    [WhiteList("static byte.CreateSaturating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteCreateSaturating<TOther>(TOther value);

    ///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
    ///<param name="value">The value that's used to create the instance of <code data-dev-comment-type="typeparamref">TSelf</code>.</param>
    ///<typeparam name="TOther">The type of <code data-dev-comment-type="paramref">value</code>.</typeparam>
    ///<returns>An instance of <code data-dev-comment-type="typeparamref">TSelf</code> created from <code data-dev-comment-type="paramref">value</code>, truncating if <code data-dev-comment-type="paramref">value</code> falls outside the representable range of <code data-dev-comment-type="typeparamref">TSelf</code>.</returns>
    [WhiteList("static byte.CreateTruncating<TOther>(TOther)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteCreateTruncating<TOther>(TOther value);

    ///<summary>Determines if a value represents an even integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an even integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.IsEvenInteger(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteIsEvenInteger(Number value);

    ///<summary>Determines if a value represents an odd integral number.</summary>
    ///<param name="value">The value to be checked.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">value</code> is an odd integer; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.IsOddInteger(byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteIsOddInteger(Number value);

    ///<summary>Tries to parse a string into a value.</summary>
    ///<param name="s">The string to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.TryParse(string, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse6(string? s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">s</code>.</returns>
    [WhiteList("static byte.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse6(Uint32Array s, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of characters into a value.</summary>
    ///<param name="s">The span of characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">s</code>.</param>
    ///<param name="result">When this method returns, contains the result of successfully parsing <code data-dev-comment-type="paramref">s</code>, or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">s</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse7(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
    [WhiteList("static byte.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse7(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="style">A bitwise combination of number styles that can be present in <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse8(Uint8Array utf8Text, System.Globalization.NumberStyles style, Intl.NumberFormat? provider, OutValue<Number> result);

    ///<summary>Parses a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<returns>The result of parsing <code data-dev-comment-type="paramref">utf8Text</code>.</returns>
    [WhiteList("static byte.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ByteParse8(Uint8Array utf8Text, Intl.NumberFormat? provider);

    ///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
    ///<param name="utf8Text">The span of UTF-8 characters to parse.</param>
    ///<param name="provider">An object that provides culture-specific formatting information about <code data-dev-comment-type="paramref">utf8Text</code>.</param>
    ///<param name="result">On return, contains the result of successfully parsing <code data-dev-comment-type="paramref">utf8Text</code> or an undefined value on failure.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">utf8Text</code> was successfully parsed; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>
    [WhiteList("static byte.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out byte)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ByteTryParse9(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);
}
