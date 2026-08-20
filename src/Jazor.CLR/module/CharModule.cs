namespace Jazor.CLR;

/// <summary>
/// System.Char 类型模块映射规则
///
/// C# char 与 JavaScript 的对应关系：
/// - C# char 是 16 位 Unicode 字符
/// - 当前编译器/宿主边界将 char 擦除为单字符 string
/// - 需要数值判断时，再在 helper 内取 UTF-16 code unit
///
/// Op 类型选择原则：
/// - Inline: 简单的单字符字符串比较和变换
/// - Alias: JS 有原生对应方法（如 toString）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: JavaScript 无对应概念或不常用
/// </summary>
[ECMAScriptModule("System/CharModule.js")]
[Jazor(Op.Alias, "char","String")]
public static class CharModule
{
	private static readonly RegExp LetterPattern = new(@"^\p{L}$", "u");
	private static readonly RegExp NumberPattern = new(@"^\p{N}$", "u");
	private static readonly RegExp PunctuationPattern = new(@"^\p{P}$", "u");
	private static readonly RegExp SeparatorPattern = new(@"^\p{Z}$", "u");
	private static readonly RegExp SymbolPattern = new(@"^\p{S}$", "u");
	private static readonly RegExp UppercaseLetterPattern = new(@"^\p{Lu}$", "u");
	private static readonly RegExp LowercaseLetterPattern = new(@"^\p{Ll}$", "u");
	private static readonly RegExp TitlecaseLetterPattern = new(@"^\p{Lt}$", "u");
	private static readonly RegExp ModifierLetterPattern = new(@"^\p{Lm}$", "u");
	private static readonly RegExp OtherLetterPattern = new(@"^\p{Lo}$", "u");
	private static readonly RegExp NonSpacingMarkPattern = new(@"^\p{Mn}$", "u");
	private static readonly RegExp SpacingCombiningMarkPattern = new(@"^\p{Mc}$", "u");
	private static readonly RegExp EnclosingMarkPattern = new(@"^\p{Me}$", "u");
	private static readonly RegExp DecimalDigitNumberPattern = new(@"^\p{Nd}$", "u");
	private static readonly RegExp LetterNumberPattern = new(@"^\p{Nl}$", "u");
	private static readonly RegExp OtherNumberPattern = new(@"^\p{No}$", "u");
	private static readonly RegExp SpaceSeparatorPattern = new(@"^\p{Zs}$", "u");
	private static readonly RegExp LineSeparatorPattern = new(@"^\p{Zl}$", "u");
	private static readonly RegExp ParagraphSeparatorPattern = new(@"^\p{Zp}$", "u");
	private static readonly RegExp ControlPattern = new(@"^\p{Cc}$", "u");
	private static readonly RegExp FormatPattern = new(@"^\p{Cf}$", "u");
	private static readonly RegExp PrivateUsePattern = new(@"^\p{Co}$", "u");
	private static readonly RegExp ConnectorPunctuationPattern = new(@"^\p{Pc}$", "u");
	private static readonly RegExp DashPunctuationPattern = new(@"^\p{Pd}$", "u");
	private static readonly RegExp OpenPunctuationPattern = new(@"^\p{Ps}$", "u");
	private static readonly RegExp ClosePunctuationPattern = new(@"^\p{Pe}$", "u");
	private static readonly RegExp InitialQuotePunctuationPattern = new(@"^\p{Pi}$", "u");
	private static readonly RegExp FinalQuotePunctuationPattern = new(@"^\p{Pf}$", "u");
	private static readonly RegExp OtherPunctuationPattern = new(@"^\p{Po}$", "u");
	private static readonly RegExp MathSymbolPattern = new(@"^\p{Sm}$", "u");
	private static readonly RegExp CurrencySymbolPattern = new(@"^\p{Sc}$", "u");
	private static readonly RegExp ModifierSymbolPattern = new(@"^\p{Sk}$", "u");
	private static readonly RegExp OtherSymbolPattern = new(@"^\p{So}$", "u");

	private static Number CompareCore(string left, string right)
	{
		var leftChar = left[0];
		var rightChar = right[0];
		return leftChar < rightChar ? -1 : (leftChar > rightChar ? 1 : 0);
	}

	private static Number GetCodeUnit(string value)
		=> value.CharCodeAt(0);

	private static Number GetCodeUnitFromChar(char value)
		=> value.ToString().CharCodeAt(0);

	private static bool IsControlCode(Number code)
		=> code < 32 || (code >= 127 && code <= 159);

	private static bool IsWhiteSpaceCode(Number code)
		=> (code >= 0x0009 && code <= 0x000D)
		|| code == 0x0020
		|| code == 0x0085
		|| code == 0x00A0
		|| code == 0x1680
		|| (code >= 0x2000 && code <= 0x200A)
		|| code == 0x2028
		|| code == 0x2029
		|| code == 0x202F
		|| code == 0x205F
		|| code == 0x3000;

	private static string GetCharacterAt(string value, Number index)
	{
		if (value == null)
			throw new Error("ArgumentNullException");
		if (Math.FloorFunc(index) != index || index < 0 || index >= value.Length)
			throw new Error("ArgumentOutOfRangeException");

		return value[(int)index].ToString();
	}

	private static Number GetUnicodeCategoryCore(string value)
	{
		// V8 property escapes classify Unicode scalar values and do not recognize an
		// isolated UTF-16 surrogate, while System.Char must report Surrogate (16).
		var codeUnit = GetCodeUnit(value);
		if (codeUnit >= 0xD800 && codeUnit <= 0xDFFF) return 16;

		if (UppercaseLetterPattern.Test(value)) return 0;
		if (LowercaseLetterPattern.Test(value)) return 1;
		if (TitlecaseLetterPattern.Test(value)) return 2;
		if (ModifierLetterPattern.Test(value)) return 3;
		if (OtherLetterPattern.Test(value)) return 4;
		if (NonSpacingMarkPattern.Test(value)) return 5;
		if (SpacingCombiningMarkPattern.Test(value)) return 6;
		if (EnclosingMarkPattern.Test(value)) return 7;
		if (DecimalDigitNumberPattern.Test(value)) return 8;
		if (LetterNumberPattern.Test(value)) return 9;
		if (OtherNumberPattern.Test(value)) return 10;
		if (SpaceSeparatorPattern.Test(value)) return 11;
		if (LineSeparatorPattern.Test(value)) return 12;
		if (ParagraphSeparatorPattern.Test(value)) return 13;
		if (ControlPattern.Test(value)) return 14;
		if (FormatPattern.Test(value)) return 15;
		if (PrivateUsePattern.Test(value)) return 17;
		if (ConnectorPunctuationPattern.Test(value)) return 18;
		if (DashPunctuationPattern.Test(value)) return 19;
		if (OpenPunctuationPattern.Test(value)) return 20;
		if (ClosePunctuationPattern.Test(value)) return 21;
		if (InitialQuotePunctuationPattern.Test(value)) return 22;
		if (FinalQuotePunctuationPattern.Test(value)) return 23;
		if (OtherPunctuationPattern.Test(value)) return 24;
		if (MathSymbolPattern.Test(value)) return 25;
		if (CurrencySymbolPattern.Test(value)) return 26;
		if (ModifierSymbolPattern.Test(value)) return 27;
		if (OtherSymbolPattern.Test(value)) return 28;
		return 29;
	}

	[Jazor(Op.Inline, "char.Char()", "\"\\0\"")]
	public extern static string _920bd6d3d675c7b2();

	/// <summary>
	/// C#: char.IsAscii(c)
	/// JS: c.charCodeAt(0) &lt;= 127
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAscii(char)", "(__arg1.charCodeAt(0) <= 127)")]
	public extern static bool _39826354b8bd0f55(string c);

	[Jazor(Op.Inline, "override char.GetHashCode()", "__arg1.charCodeAt(0)")]
	public extern static Number _5b81ebfb78d5415c(string instance);

	/// <summary>
	/// C#: char.Equals(obj)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override char.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _3f176ca2992b307c(string instance, object? obj);

	/// <summary>
	/// C#: char.Equals(other)
	/// JS: instance === other
	/// </summary>
	[Jazor(Op.Inline, "char.Equals(char)", "(__arg1 === __arg2)")]
	public extern static bool _632690bee0e71964(string instance, string obj);

	/// <summary>
	/// C#: char.CompareTo(obj)
	/// JS: 与 .NET 一致的 CompareTo 规则，单独处理 null 和类型检查
	/// </summary>
	[Jazor(Op.Import, "char.CompareTo(object)")]
	public static Number _ddf9c5affdc041df(string instance, object? value)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "string")
			throw new Error("ArgumentException: Object must be of type Char.");

		return CompareCore(instance, (string)value);
	}

	/// <summary>
	/// C#: char.CompareTo(other)
	/// JS: 返回负数、零或正数
	/// </summary>
	[Jazor(Op.Inline, "char.CompareTo(char)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _309d33b86c3815d8(string instance, string value);

	/// <summary>
	/// C#: char.ToString()
	/// JS: instance
	/// </summary>
	[Jazor(Op.Inline, "override char.ToString()", "__arg1")]
	public extern static string _4861ba21870a2ec3(string instance);

	[Jazor(Op.Inline, "char.ToString(System.IFormatProvider)", "__arg1")]
	public extern static string _fc3c2436fe7b6197(string instance, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: char.ToString(c)
	/// JS: c
	/// </summary>
	[Jazor(Op.Inline, "static char.ToString(char)", "__arg1")]
	public extern static string _f59d4d8b2c441c53(string c);

	/// <summary>
	/// C#: char.Parse(s)
	/// JS: 需要验证字符串长度为1，返回单字符 string
	/// </summary>
	[Jazor(Op.Import, "static char.Parse(string)")]
	public static string _d89999df761a6d2e(string s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (s.Length != 1)
			throw new Error("FormatException: String must be exactly one character long.");
		return s.Substring(0, 1);
	}

	/// <summary>
	/// C#: char.TryParse(s, out result)
	/// JS: 返回 [success, char]
	/// </summary>
	[Jazor(Op.Import, "static char.TryParse(string, out char)")]
	public static Array<object?> _9450f84427428db0(string? s, string result)
	{
		if (s != null && s.Length == 1)
			return [true, s.Substring(0, 1)];
		return [false, "\0"];
	}

	/// <summary>
	/// C#: char.IsAsciiLetter(c)
	/// JS: A-Z / a-z
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetter(char)", "((__arg1 >= \"A\" && __arg1 <= \"Z\") || (__arg1 >= \"a\" && __arg1 <= \"z\"))")]
	public extern static bool _1737fc6cbaca1038(string c);

	/// <summary>
	/// C#: char.IsAsciiLetterLower(c)
	/// JS: a-z
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterLower(char)", "(__arg1 >= \"a\" && __arg1 <= \"z\")")]
	public extern static bool _d0f415a83ae10d8a(string c);

	/// <summary>
	/// C#: char.IsAsciiLetterUpper(c)
	/// JS: A-Z
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterUpper(char)", "(__arg1 >= \"A\" && __arg1 <= \"Z\")")]
	public extern static bool _30f49ccd6f1f8b2d(string c);

	/// <summary>
	/// C#: char.IsAsciiDigit(c)
	/// JS: 0-9
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiDigit(char)", "(__arg1 >= \"0\" && __arg1 <= \"9\")")]
	public extern static bool _266ce5f0f0db2958(string c);

	/// <summary>
	/// C#: char.IsAsciiLetterOrDigit(c)
	/// JS: IsAsciiLetter(c) || IsAsciiDigit(c)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiLetterOrDigit(char)", "((__arg1 >= \"A\" && __arg1 <= \"Z\") || (__arg1 >= \"a\" && __arg1 <= \"z\") || (__arg1 >= \"0\" && __arg1 <= \"9\"))")]
	public extern static bool _3f3a99864b7042e9(string c);

	/// <summary>
	/// C#: char.IsAsciiHexDigit(c)
	/// JS: 0-9 / A-F / a-f
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigit(char)", "((__arg1 >= \"0\" && __arg1 <= \"9\") || (__arg1 >= \"A\" && __arg1 <= \"F\") || (__arg1 >= \"a\" && __arg1 <= \"f\"))")]
	public extern static bool _8ebed700a57241d2(string c);

	/// <summary>
	/// C#: char.IsAsciiHexDigitUpper(c)
	/// JS: 0-9 / A-F
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigitUpper(char)", "((__arg1 >= \"0\" && __arg1 <= \"9\") || (__arg1 >= \"A\" && __arg1 <= \"F\"))")]
	public extern static bool _47cc49555e21ab3b(string c);

	/// <summary>
	/// C#: char.IsAsciiHexDigitLower(c)
	/// JS: 0-9 / a-f
	/// </summary>
	[Jazor(Op.Inline, "static char.IsAsciiHexDigitLower(char)", "((__arg1 >= \"0\" && __arg1 <= \"9\") || (__arg1 >= \"a\" && __arg1 <= \"f\"))")]
	public extern static bool _c082c46f951a0c9f(string c);

	/// <summary>
	/// C#: char.IsDigit(c)
	/// JS: 仅支持 ASCII 数字
	/// </summary>
	[Jazor(Op.Import, "static char.IsDigit(char)")]
	public static bool _91a882221d295c32(string c)
		=> DecimalDigitNumberPattern.Test(c);

	/// <summary>
	/// C#: char.IsBetween(c, min, max)
	/// JS: c &gt;= min &amp;&amp; c &lt;= max
	/// </summary>
	[Jazor(Op.Inline, "static char.IsBetween(char, char, char)", "(__arg1 >= __arg2 && __arg1 <= __arg3)")]
	public extern static bool _dfb76865a7840d43(string c, string minInclusive, string maxInclusive);

	/// <summary>
	/// C#: char.IsLetter(c)
	/// JS: /[a-zA-Z]/.test(c)
	/// </summary>
	[Jazor(Op.Import, "static char.IsLetter(char)")]
	public static bool _38721338a529a8d7(string c)
		=> LetterPattern.Test(c);

	/// <summary>
	/// C#: char.IsWhiteSpace(c)
	/// JS: 对齐 .NET 当前 BMP whitespace 集合
	/// </summary>
	[Jazor(Op.Import, "static char.IsWhiteSpace(char)")]
	public static bool _16e351e6f7b127f7(string c)
		=> IsWhiteSpaceCode(GetCodeUnit(c));

	/// <summary>
	/// C#: char.IsUpper(c)
	/// JS: /[A-Z]/.test(c)
	/// </summary>
	[Jazor(Op.Import, "static char.IsUpper(char)")]
	public static bool _7d70d8021ab255a8(string c)
		=> UppercaseLetterPattern.Test(c);

	/// <summary>
	/// C#: char.IsLower(c)
	/// JS: /[a-z]/.test(c)
	/// </summary>
	[Jazor(Op.Import, "static char.IsLower(char)")]
	public static bool _b344d14ce0e33570(string c)
		=> LowercaseLetterPattern.Test(c);

	/// <summary>
	/// C#: char.IsPunctuation(c)
	/// JS: 常见标点符号范围检查
	/// </summary>
	[Jazor(Op.Import, "static char.IsPunctuation(char)")]
	public static bool _ce3de1c060963041(string c)
		=> PunctuationPattern.Test(c);

	/// <summary>
	/// C#: char.IsLetterOrDigit(c)
	/// JS: /[a-zA-Z0-9]/.test(c)
	/// </summary>
	[Jazor(Op.Import, "static char.IsLetterOrDigit(char)")]
	public static bool _49432dd2165d98f0(string c)
		=> LetterPattern.Test(c) || DecimalDigitNumberPattern.Test(c);

	/// <summary>
	/// C#: char.ToUpper(c, culture)
	/// JS: c.toUpperCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpper(char, System.Globalization.CultureInfo)", "__arg1.toUpperCase()")]
	public extern static string _dd41639bb00c83ab(string c, String culture);

	/// <summary>
	/// C#: char.ToUpper(c)
	/// JS: c.toUpperCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpper(char)", "__arg1.toUpperCase()")]
	public extern static string _2713512e6f5a9312(string c);

	/// <summary>
	/// C#: char.ToUpperInvariant(c)
	/// JS: c.toUpperCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToUpperInvariant(char)", "__arg1.toUpperCase()")]
	public extern static string _b0c91aa30cd2a5f7(string c);

	/// <summary>
	/// C#: char.ToLower(c, culture)
	/// JS: c.toLowerCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLower(char, System.Globalization.CultureInfo)", "__arg1.toLowerCase()")]
	public extern static string _b81ddeb8c6240b72(string c, String culture);

	/// <summary>
	/// C#: char.ToLower(c)
	/// JS: c.toLowerCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLower(char)", "__arg1.toLowerCase()")]
	public extern static string _b91d21a936e68017(string c);

	/// <summary>
	/// C#: char.ToLowerInvariant(c)
	/// JS: c.toLowerCase()
	/// </summary>
	[Jazor(Op.Inline, "static char.ToLowerInvariant(char)", "__arg1.toLowerCase()")]
	public extern static string _76274ed9d45c0127(string c);

	[Jazor(Op.Inline, "char.GetTypeCode()", "4")]
	public extern static System.TypeCode _84932c09c59d9b51(string instance);

	/// <summary>
	/// C#: char.IsControl(c)
	/// JS: control code ranges U+0000..U+001F and U+007F..U+009F
	/// </summary>
	[Jazor(Op.Import, "static char.IsControl(char)")]
	public static bool _c12d0a40e2ed8650(string c)
		=> IsControlCode(GetCodeUnit(c));

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
		var c = GetCodeUnitFromChar(s[(int)index]);
		return IsControlCode(c);
	}

	/// <summary>
	/// C#: char.IsDigit(s, index)
	/// JS: IsDigit(s.charCodeAt(index))
	/// </summary>
	[Jazor(Op.Import, "static char.IsDigit(string, int)")]
	public static bool _52eb020022da112b(string s, Number index)
		=> DecimalDigitNumberPattern.Test(GetCharacterAt(s, index));

	[Jazor(Op.Import, "static char.IsLetter(string, int)")]
	public static bool _e7ee64c732d21cd5(string s, Number index)
		=> LetterPattern.Test(GetCharacterAt(s, index));

	[Jazor(Op.Import, "static char.IsLetterOrDigit(string, int)")]
	public static bool _d752ce4eaadf7612(string s, Number index)
	{
		var value = GetCharacterAt(s, index);
		return LetterPattern.Test(value) || DecimalDigitNumberPattern.Test(value);
	}

	/// <summary>
	/// C#: char.IsLower(s, index)
	/// JS: String.fromCharCode(s.charCodeAt(index)).toLowerCase() === String.fromCharCode(s.charCodeAt(index))
	/// 简化：仅检查 ASCII 小写
	/// </summary>
	[Jazor(Op.Import, "static char.IsLower(string, int)")]
	public static bool _6ebe08db86ea37a2(string s, Number index)
		=> LowercaseLetterPattern.Test(GetCharacterAt(s, index));

	[Jazor(Op.Import, "static char.IsNumber(char)")]
	public static bool _77e97c648607e65e(string c)
		=> NumberPattern.Test(c);

	[Jazor(Op.Import, "static char.IsNumber(string, int)")]
	public static bool _5180e5acb1d4bcb0(string s, Number index)
		=> NumberPattern.Test(GetCharacterAt(s, index));

	[Jazor(Op.Import, "static char.IsPunctuation(string, int)")]
	public static bool _5f7e394ed1d09372(string s, Number index)
		=> PunctuationPattern.Test(GetCharacterAt(s, index));

	[Jazor(Op.Import, "static char.IsSeparator(char)")]
	public static bool _066fc76a18dc824f(string c)
		=> SeparatorPattern.Test(c);

	[Jazor(Op.Import, "static char.IsSeparator(string, int)")]
	public static bool _3d391ade47da71a6(string s, Number index)
		=> SeparatorPattern.Test(GetCharacterAt(s, index));

	/// <summary>
	/// C#: char.IsSurrogate(c)
	/// JS: c &gt;= 0xD800 &amp;&amp; c &lt;= 0xDFFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsSurrogate(char)", "(__arg1.charCodeAt(0) >= 55296 && __arg1.charCodeAt(0) <= 57343)")]
	public extern static bool _e5949fe4a1738a38(string c);

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
		var c = GetCodeUnitFromChar(s[(int)index]);
		return c >= 55296 && c <= 57343;
	}

	[Jazor(Op.Import, "static char.IsSymbol(char)")]
	public static bool _0f18b1b6d2524322(string c)
		=> SymbolPattern.Test(c);

	[Jazor(Op.Import, "static char.IsSymbol(string, int)")]
	public static bool _16587492d280e91d(string s, Number index)
		=> SymbolPattern.Test(GetCharacterAt(s, index));

	/// <summary>
	/// C#: char.IsUpper(s, index)
	/// JS: 仅检查 ASCII 大写
	/// </summary>
	[Jazor(Op.Import, "static char.IsUpper(string, int)")]
	public static bool _1ae24de44f4b499e(string s, Number index)
		=> UppercaseLetterPattern.Test(GetCharacterAt(s, index));

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
		return IsWhiteSpaceCode(GetCodeUnitFromChar(s[(int)index]));
	}

	[Jazor(Op.Import, "static char.GetUnicodeCategory(char)")]
	public static Number _226cc4ffd552fcf9(string c)
		=> GetUnicodeCategoryCore(c);

	[Jazor(Op.Import, "static char.GetUnicodeCategory(string, int)")]
	public static Number _e41ad686bd01aff1(string s, Number index)
		=> GetUnicodeCategoryCore(GetCharacterAt(s, index));

	/// <summary>
	/// C#: char.GetNumericValue(c)
	/// JS: 仅对 ASCII 数字有效
	/// </summary>
	[Jazor(Op.Import, "static char.GetNumericValue(char)")]
	public static Number _d86c1e9964250116(string c)
	{
		var code = GetCodeUnit(c);
		if (code >= 48 && code <= 57)
			return code - 48;
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
		var c = GetCodeUnitFromChar(s[(int)index]);
		if (c >= 48 && c <= 57)
			return c - 48;
		return -1;
	}

	/// <summary>
	/// C#: char.IsHighSurrogate(c)
	/// JS: c &gt;= 0xD800 &amp;&amp; c &lt;= 0xDBFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsHighSurrogate(char)", "(__arg1.charCodeAt(0) >= 55296 && __arg1.charCodeAt(0) <= 56319)")]
	public extern static bool _4c066834beda061c(string c);

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
		var c = GetCodeUnitFromChar(s[(int)index]);
		return c >= 55296 && c <= 56319;
	}

	/// <summary>
	/// C#: char.IsLowSurrogate(c)
	/// JS: c &gt;= 0xDC00 &amp;&amp; c &lt;= 0xDFFF
	/// </summary>
	[Jazor(Op.Inline, "static char.IsLowSurrogate(char)", "(__arg1.charCodeAt(0) >= 56320 && __arg1.charCodeAt(0) <= 57343)")]
	public extern static bool _7761ca7b99042e8a(string c);

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
		var c = GetCodeUnitFromChar(s[(int)index]);
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
		var c1 = GetCodeUnitFromChar(s[(int)index]);
		var c2 = GetCodeUnitFromChar(s[(int)index + 1]);
		return (c1 >= 55296 && c1 <= 56319) && (c2 >= 56320 && c2 <= 57343);
	}

	/// <summary>
	/// C#: char.IsSurrogatePair(highSurrogate, lowSurrogate)
	/// JS: IsHighSurrogate(highSurrogate) &amp;&amp; IsLowSurrogate(lowSurrogate)
	/// </summary>
	[Jazor(Op.Inline, "static char.IsSurrogatePair(char, char)", "((__arg1.charCodeAt(0) >= 55296 && __arg1.charCodeAt(0) <= 56319) && (__arg2.charCodeAt(0) >= 56320 && __arg2.charCodeAt(0) <= 57343))")]
	public extern static bool _efe9c9b601517069(string highSurrogate, string lowSurrogate);

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
	[Jazor(Op.Inline, "static char.ConvertToUtf32(char, char)", "(((__arg1.charCodeAt(0) - 55296) << 10) + (__arg2.charCodeAt(0) - 56320) + 65536)")]
	public extern static Number _f842e9b2f7fea133(string highSurrogate, string lowSurrogate);

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

		var c = GetCodeUnitFromChar(s[(int)index]);
		// 检查是否是高代理项
		if (c >= 55296 && c <= 56319)
		{
			// 需要低代理项
			if ((int)index + 1 >= s.Length)
				throw new Error("ArgumentException: Missing low surrogate");
			var low = GetCodeUnitFromChar(s[(int)index + 1]);
			if (low < 56320 || low > 57343)
				throw new Error("ArgumentException: Invalid low surrogate");
			return ((c - 55296) << 10) + (low - 56320) + 65536;
		}
		return c;
	}
}
