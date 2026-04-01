namespace Jazor.CLR;

/// <summary>
/// System.Char 类型模块映射规则
///
/// C# char 与 JavaScript 的对应关系：
/// - C# char 是 16 位 Unicode 字符
/// - JavaScript 中映射为 Number（字符码）或单字符 string
/// - 使用 char code 进行数值比较
///
/// Op 类型选择原则：
/// - Inline: 简单的字符码比较和运算
/// - Alias: JS 有原生对应方法（如 toString）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: JavaScript 无对应概念或不常用
/// </summary>
[ECMAScriptModule("System/CharModule.js")]
[Jazor(Op.Alias, "char","String")]
public static class CharModule
{
	[Jazor(Op.Discard, "char.Char()")]
	public extern static Number _920bd6d3d675c7b2();

	/// <summary>
	/// C#: char.IsAscii(c)
	/// JS: c &lt;= 127
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAscii(char)", "(__arg1 <= 127)")]
	public extern static bool _39826354b8bd0f55(Number c);

	[Jazor(Op.Discard, "override char.GetHashCode()")]
	public extern static Number _5b81ebfb78d5415c(Number instance);

	/// <summary>
	/// C#: char.Equals(obj)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override char.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _3f176ca2992b307c(Number instance, object? obj);

	/// <summary>
	/// C#: char.Equals(other)
	/// JS: instance === other
	/// </summary>
	[Jazor(Op.Inline, "char.Equals(char)", "(__arg1 === __arg2)")]
	public extern static bool _632690bee0e71964(Number instance, Number obj);

	/// <summary>
	/// C#: char.CompareTo(obj)
	/// JS: instance - (obj as number)
	/// </summary>
	[Jazor(Op.Inline, "char.CompareTo(object)", "(__arg1 - (__arg2 ?? 0))")]
	public extern static Number _ddf9c5affdc041df(Number instance, object? value);

	/// <summary>
	/// C#: char.CompareTo(other)
	/// JS: instance - other
	/// </summary>
	[Jazor(Op.Inline, "char.CompareTo(char)", "(__arg1 - __arg2)")]
	public extern static Number _309d33b86c3815d8(Number instance, Number value);

	/// <summary>
	/// C#: char.ToString()
	/// JS: String.fromCharCode(instance)
	/// </summary>
	[Jazor(Op.Inline, "override char.ToString()", "String.fromCharCode(__arg1)")]
	public extern static string _4861ba21870a2ec3(Number instance);

	[Jazor(Op.Discard, "char.ToString(System.IFormatProvider)")]
	public extern static string _fc3c2436fe7b6197(Number instance, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: char.ToString(c)
	/// JS: String.fromCharCode(c)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToString(char)", "String.fromCharCode(__arg1)")]
	public extern static string _f59d4d8b2c441c53(Number c);

	/// <summary>
	/// C#: char.Parse(s)
	/// JS: 需要验证字符串长度为1，返回 char code
	/// </summary>
	[Jazor(Op.Import, "static char.Parse(string)")]
	public static Number _d89999df761a6d2e(string s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (s.Length != 1)
			throw new Error("FormatException: String must be exactly one character long.");
		return (Number)s[0];
	}

	/// <summary>
	/// C#: char.TryParse(s, out result)
	/// JS: 返回 [success, charCode]
	/// </summary>
	[Jazor(Op.Import, "static char.TryParse(string, out char)")]
	public static Array<object?> _9450f84427428db0(string? s, Number result)
	{
		if (s != null && s.Length == 1)
			return [true, (Number)s[0]];
		return [false, 0];
	}

	/// <summary>
	/// C#: char.IsAsciiLetter(c)
	/// JS: (c &gt;= 65 &amp;&amp; c &lt;= 90) || (c &gt;= 97 &amp;&amp; c &lt;= 122)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetter(char)", "((__arg1 >= 65 && __arg1 <= 90) || (__arg1 >= 97 && __arg1 <= 122))")]
	public extern static bool _1737fc6cbaca1038(Number c);

	/// <summary>
	/// C#: char.IsAsciiLetterLower(c)
	/// JS: c &gt;= 97 &amp;&amp; c &lt;= 122
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterLower(char)", "(__arg1 >= 97 && __arg1 <= 122)")]
	public extern static bool _d0f415a83ae10d8a(Number c);

	/// <summary>
	/// C#: char.IsAsciiLetterUpper(c)
	/// JS: c &gt;= 65 &amp;&amp; c &lt;= 90
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterUpper(char)", "(__arg1 >= 65 && __arg1 <= 90)")]
	public extern static bool _30f49ccd6f1f8b2d(Number c);

	/// <summary>
	/// C#: char.IsAsciiDigit(c)
	/// JS: c &gt;= 48 &amp;&amp; c &lt;= 57
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiDigit(char)", "(__arg1 >= 48 && __arg1 <= 57)")]
	public extern static bool _266ce5f0f0db2958(Number c);

	/// <summary>
	/// C#: char.IsAsciiLetterOrDigit(c)
	/// JS: IsAsciiLetter(c) || IsAsciiDigit(c)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterOrDigit(char)", "((__arg1 >= 65 && __arg1 <= 90) || (__arg1 >= 97 && __arg1 <= 122) || (__arg1 >= 48 && __arg1 <= 57))")]
	public extern static bool _3f3a99864b7042e9(Number c);

	/// <summary>
	/// C#: char.IsAsciiHexDigit(c)
	/// JS: (c &gt;= 48 &amp;&amp; c &lt;= 57) || (c &gt;= 65 &amp;&amp; c &lt;= 70) || (c &gt;= 97 &amp;&amp; c &lt;= 102)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigit(char)", "((__arg1 >= 48 && __arg1 <= 57) || (__arg1 >= 65 && __arg1 <= 70) || (__arg1 >= 97 && __arg1 <= 102))")]
	public extern static bool _8ebed700a57241d2(Number c);

	/// <summary>
	/// C#: char.IsAsciiHexDigitUpper(c)
	/// JS: (c &gt;= 48 &amp;&amp; c &lt;= 57) || (c &gt;= 65 &amp;&amp; c &lt;= 70)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigitUpper(char)", "((__arg1 >= 48 && __arg1 <= 57) || (__arg1 >= 65 && __arg1 <= 70))")]
	public extern static bool _47cc49555e21ab3b(Number c);

	/// <summary>
	/// C#: char.IsAsciiHexDigitLower(c)
	/// JS: (c &gt;= 48 &amp;&amp; c &lt;= 57) || (c &gt;= 97 &amp;&amp; c &lt;= 102)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigitLower(char)", "((__arg1 >= 48 && __arg1 <= 57) || (__arg1 >= 97 && __arg1 <= 102))")]
	public extern static bool _c082c46f951a0c9f(Number c);

	/// <summary>
	/// C#: char.IsDigit(c)
	/// JS: 使用正则 /\d/.test(String.fromCharCode(c))
	/// 简化实现：仅支持 ASCII 数字
	/// </summary>
	[Jazor(Op.Inline, "static char.IsDigit(char)", "(__arg1 >= 48 && __arg1 <= 57)")]
	public extern static bool _91a882221d295c32(Number c);

	/// <summary>
	/// C#: char.IsBetween(c, min, max)
	/// JS: c &gt;= min &amp;&amp; c &lt;= max
	/// </summary>
	[Jazor(Op.Inline, "static char.IsBetween(char, char, char)", "(__arg1 >= __arg2 && __arg1 <= __arg3)")]
	public extern static bool _dfb76865a7840d43(Number c, Number minInclusive, Number maxInclusive);

	/// <summary>
	/// C#: char.IsLetter(c)
	/// JS: /[a-zA-Z]/.test(String.fromCharCode(c))
	/// </summary>
	[Jazor(Op.Inline, "static char.IsLetter(char)", "/[a-zA-Z]/.test(String.fromCharCode(__arg1))")]
	public extern static bool _38721338a529a8d7(Number c);

	/// <summary>
	/// C#: char.IsWhiteSpace(c)
	/// JS: /\s/.test(String.fromCharCode(c))
	/// 简化实现：常见空白字符
	/// </summary>
	[Jazor(Op.Inline, "static char.IsWhiteSpace(char)", "(__arg1 == 32 || __arg1 == 9 || __arg1 == 10 || __arg1 == 13 || __arg1 == 12)")]
	public extern static bool _16e351e6f7b127f7(Number c);

	/// <summary>
	/// C#: char.IsUpper(c)
	/// JS: /[A-Z]/.test(String.fromCharCode(c))
	/// </summary>
	[Jazor(Op.Inline, "static char.IsUpper(char)", "/[A-Z]/.test(String.fromCharCode(__arg1))")]
	public extern static bool _7d70d8021ab255a8(Number c);

	/// <summary>
	/// C#: char.IsLower(c)
	/// JS: /[a-z]/.test(String.fromCharCode(c))
	/// </summary>
	[Jazor(Op.Inline, "static char.IsLower(char)", "/[a-z]/.test(String.fromCharCode(__arg1))")]
	public extern static bool _b344d14ce0e33570(Number c);

	/// <summary>
	/// C#: char.IsPunctuation(c)
	/// JS: 常见标点符号范围检查
	/// </summary>
	[Jazor(Op.Inline, "static char.IsPunctuation(char)", "/[!\\\"#$%&'()*+,-./:;<=>?@[\\\\]^_`{|}~]/.test(String.fromCharCode(__arg1))")]
	public extern static bool _ce3de1c060963041(Number c);

	/// <summary>
	/// C#: char.IsLetterOrDigit(c)
	/// JS: /[a-zA-Z0-9]/.test(String.fromCharCode(c))
	/// </summary>
	[Jazor(Op.Inline, "static char.IsLetterOrDigit(char)", "/[a-zA-Z0-9]/.test(String.fromCharCode(__arg1))")]
	public extern static bool _49432dd2165d98f0(Number c);

	/// <summary>
	/// C#: char.ToUpper(c, culture)
	/// JS: String.fromCharCode(c).toUpperCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpper(char, System.Globalization.CultureInfo)", "String.fromCharCode(__arg1).toUpperCase().charCodeAt(0)")]
	public extern static Number _dd41639bb00c83ab(Number c, String culture);

	/// <summary>
	/// C#: char.ToUpper(c)
	/// JS: String.fromCharCode(c).toUpperCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpper(char)", "String.fromCharCode(__arg1).toUpperCase().charCodeAt(0)")]
	public extern static Number _2713512e6f5a9312(Number c);

	/// <summary>
	/// C#: char.ToUpperInvariant(c)
	/// JS: String.fromCharCode(c).toUpperCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpperInvariant(char)", "String.fromCharCode(__arg1).toUpperCase().charCodeAt(0)")]
	public extern static Number _b0c91aa30cd2a5f7(Number c);

	/// <summary>
	/// C#: char.ToLower(c, culture)
	/// JS: String.fromCharCode(c).toLowerCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLower(char, System.Globalization.CultureInfo)", "String.fromCharCode(__arg1).toLowerCase().charCodeAt(0)")]
	public extern static Number _b81ddeb8c6240b72(Number c, String culture);

	/// <summary>
	/// C#: char.ToLower(c)
	/// JS: String.fromCharCode(c).toLowerCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLower(char)", "String.fromCharCode(__arg1).toLowerCase().charCodeAt(0)")]
	public extern static Number _b91d21a936e68017(Number c);

	/// <summary>
	/// C#: char.ToLowerInvariant(c)
	/// JS: String.fromCharCode(c).toLowerCase().charCodeAt(0)
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLowerInvariant(char)", "String.fromCharCode(__arg1).toLowerCase().charCodeAt(0)")]
	public extern static Number _76274ed9d45c0127(Number c);

	[Jazor(Op.Discard, "char.GetTypeCode()")]
	public extern static System.TypeCode _84932c09c59d9b51(Number instance);

	/// <summary>
	/// C#: char.IsControl(c)
	/// JS: c &lt; 32 || c === 127
	/// </summary>
	[Jazor(Op.Inline, "static char.IsControl(char)", "(__arg1 < 32 || __arg1 == 127)")]
	public extern static bool _c12d0a40e2ed8650(Number c);

	/// <summary>
	/// C#: char.IsControl(s, index)
	/// JS: IsControl(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsControl(string, int)")]
	public static bool _68e189abbb5497dc(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c < 32 || c == 127;
	}

	/// <summary>
	/// C#: char.IsDigit(s, index)
	/// JS: IsDigit(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsDigit(string, int)")]
	public static bool _52eb020022da112b(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 48 && c <= 57;
	}

	[Jazor(Op.Discard, "static char.IsLetter(string, int)")]
	public extern static bool _e7ee64c732d21cd5(string s, Number index);

	[Jazor(Op.Discard, "static char.IsLetterOrDigit(string, int)")]
	public extern static bool _d752ce4eaadf7612(string s, Number index);

	/// <summary>
	/// C#: char.IsLower(s, index)
	/// JS: String.fromCharCode(s.charCodeAt(index)).toLowerCase() === String.fromCharCode(s.charCodeAt(index))
	/// 简化：仅检查 ASCII 小写
	/// </summary>
	[Jazor(Op.Import, "static char.IsLower(string, int)")]
	public static bool _6ebe08db86ea37a2(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 97 && c <= 122;
	}

	[Jazor(Op.Discard, "static char.IsNumber(char)")]
	public extern static bool _77e97c648607e65e(Number c);

	[Jazor(Op.Discard, "static char.IsNumber(string, int)")]
	public extern static bool _5180e5acb1d4bcb0(string s, Number index);

	[Jazor(Op.Discard, "static char.IsPunctuation(string, int)")]
	public extern static bool _5f7e394ed1d09372(string s, Number index);

	[Jazor(Op.Discard, "static char.IsSeparator(char)")]
	public extern static bool _066fc76a18dc824f(Number c);

	[Jazor(Op.Discard, "static char.IsSeparator(string, int)")]
	public extern static bool _3d391ade47da71a6(string s, Number index);

	/// <summary>
	/// C#: char.IsSurrogate(c)
	/// JS: c &gt;= 0xD800 &amp;&amp; c &lt;= 0xDFFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsSurrogate(char)", "(__arg1 >= 55296 && __arg1 <= 57343)")]
	public extern static bool _e5949fe4a1738a38(Number c);

	/// <summary>
	/// C#: char.IsSurrogate(s, index)
	/// JS: IsSurrogate(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsSurrogate(string, int)")]
	public static bool _bca1b50c85e48723(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 55296 && c <= 57343;
	}

	[Jazor(Op.Discard, "static char.IsSymbol(char)")]
	public extern static bool _0f18b1b6d2524322(Number c);

	[Jazor(Op.Discard, "static char.IsSymbol(string, int)")]
	public extern static bool _16587492d280e91d(string s, Number index);

	/// <summary>
	/// C#: char.IsUpper(s, index)
	/// JS: 仅检查 ASCII 大写
	/// </summary>
	[Jazor(Op.Import, "static char.IsUpper(string, int)")]
	public static bool _1ae24de44f4b499e(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 65 && c <= 90;
	}

	/// <summary>
	/// C#: char.IsWhiteSpace(s, index)
	/// JS: 检查常见空白字符
	/// </summary>
	[Jazor(Op.Import, "static char.IsWhiteSpace(string, int)")]
	public static bool _a21dd6de62be7b75(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c == 32 || c == 9 || c == 10 || c == 13 || c == 12;
	}

	[Jazor(Op.Discard, "static char.GetUnicodeCategory(char)")]
	public extern static System.Globalization.UnicodeCategory _226cc4ffd552fcf9(Number c);

	[Jazor(Op.Discard, "static char.GetUnicodeCategory(string, int)")]
	public extern static System.Globalization.UnicodeCategory _e41ad686bd01aff1(string s, Number index);

	/// <summary>
	/// C#: char.GetNumericValue(c)
	/// JS: 仅对 ASCII 数字有效
	/// </summary>
	[Jazor(Op.Import, "static char.GetNumericValue(char)")]
	public static Number _d86c1e9964250116(Number c)
	{
		if (c >= 48 && c <= 57)
			return c - 48;
		return -1;
	}

	/// <summary>
	/// C#: char.GetNumericValue(s, index)
	/// JS: GetNumericValue(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.GetNumericValue(string, int)")]
	public static Number _938251f1b1fc7bc8(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		if (c >= 48 && c <= 57)
			return c - 48;
		return -1;
	}

	/// <summary>
	/// C#: char.IsHighSurrogate(c)
	/// JS: c &gt;= 0xD800 &amp;&amp; c &lt;= 0xDBFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsHighSurrogate(char)", "(__arg1 >= 55296 && __arg1 <= 56319)")]
	public extern static bool _4c066834beda061c(Number c);

	/// <summary>
	/// C#: char.IsHighSurrogate(s, index)
	/// JS: IsHighSurrogate(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsHighSurrogate(string, int)")]
	public static bool _311485d1745ce294(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 55296 && c <= 56319;
	}

	/// <summary>
	/// C#: char.IsLowSurrogate(c)
	/// JS: c &gt;= 0xDC00 &amp;&amp; c &lt;= 0xDFFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsLowSurrogate(char)", "(__arg1 >= 56320 && __arg1 <= 57343)")]
	public extern static bool _7761ca7b99042e8a(Number c);

	/// <summary>
	/// C#: char.IsLowSurrogate(s, index)
	/// JS: IsLowSurrogate(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsLowSurrogate(string, int)")]
	public static bool _1d56cdc9a261e948(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");
		var c = (Number)s[(int)index];
		return c >= 56320 && c <= 57343;
	}

	/// <summary>
	/// C#: char.IsSurrogatePair(s, index)
	/// JS: IsHighSurrogate(s.charCodeAt(index)) &amp;&amp; IsLowSurrogate(s.charCodeAt(index + 1))
	/// </summary>
	[Jazor(Op.Import, "static char.IsSurrogatePair(string, int)")]
	public static bool _27c9fca9c829cc5e(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length - 1)
			return false;
		var c1 = (Number)s[(int)index];
		var c2 = (Number)s[(int)index + 1];
		return (c1 >= 55296 && c1 <= 56319) && (c2 >= 56320 && c2 <= 57343);
	}

	/// <summary>
	/// C#: char.IsSurrogatePair(highSurrogate, lowSurrogate)
	/// JS: IsHighSurrogate(highSurrogate) &amp;&amp; IsLowSurrogate(lowSurrogate)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsSurrogatePair(char, char)", "((__arg1 >= 55296 && __arg1 <= 56319) && (__arg2 >= 56320 && __arg2 <= 57343))")]
	public extern static bool _efe9c9b601517069(Number highSurrogate, Number lowSurrogate);

	/// <summary>
	/// C#: char.ConvertFromUtf32(utf32)
	/// JS: String.fromCodePoint(utf32)
	/// </summary>
	[Jazor(Op.Inline, "static char.ConvertFromUtf32(int)", "String.fromCodePoint(__arg1)")]
	public extern static string _fdcbb676a7d83aab(Number utf32);

	/// <summary>
	/// C#: char.ConvertToUtf32(highSurrogate, lowSurrogate)
	/// JS: ((highSurrogate - 0xD800) &lt;&lt; 10) + (lowSurrogate - 0xDC00) + 0x10000
	/// </summary>
	[Jazor(Op.Inline, "static char.ConvertToUtf32(char, char)", "(((__arg1 - 55296) << 10) + (__arg2 - 56320) + 65536)")]
	public extern static Number _f842e9b2f7fea133(Number highSurrogate, Number lowSurrogate);

	/// <summary>
	/// C#: char.ConvertToUtf32(s, index)
	/// JS: 检查代理对并计算 code point
	/// </summary>
	[Jazor(Op.Import, "static char.ConvertToUtf32(string, int)")]
	public static Number _d9f7c3c03ea64580(string s, Number index)
	{
		if (s == null)
			throw new Error("ArgumentNullException");
		if (index < 0 || index >= s.Length)
			throw new Error("ArgumentOutOfRangeException");

		var c = (Number)s[(int)index];
		// 检查是否是高代理项
		if (c >= 55296 && c <= 56319)
		{
			// 需要低代理项
			if ((int)index + 1 >= s.Length)
				throw new Error("ArgumentException: Missing low surrogate");
			var low = (Number)s[(int)index + 1];
			if (low < 56320 || low > 57343)
				throw new Error("ArgumentException: Invalid low surrogate");
			return ((c - 55296) << 10) + (low - 56320) + 65536;
		}
		return c;
	}
}
