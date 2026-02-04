using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("char", WhiteListOp.Allowed, null,"System/CharModule.js")]
public static class CharModule
{
	//char.MaxValue = ;

	//char.MinValue = ;

	[WhiteList("char.Char()", WhiteListOp.Discard)]
	public extern static Number _920bd6d3d675c7b2();

	///<summary>Returns <see langword="true" /> if <paramref name="c" /> is an ASCII character ([ U+0000..U+007F ]).</summary>
	[WhiteList("static char.IsAscii(char)", WhiteListOp.Discard)]
	public extern static bool _39826354b8bd0f55(Number c);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override char.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _5b81ebfb78d5415c(Number instance);

	///<summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	[WhiteList("override char.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _3f176ca2992b307c(Number instance, Object? obj);

	///<summary>Returns a value that indicates whether this instance is equal to the specified <see cref="T:System.Char" /> object.</summary>
	[WhiteList("char.Equals(char)", WhiteListOp.Discard)]
	public extern static bool _632690bee0e71964(Number instance, Number obj);

	///<summary>Compares this instance to a specified object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	[WhiteList("char.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _ddf9c5affdc041df(Number instance, Object? value);

	///<summary>Compares this instance to a specified <see cref="T:System.Char" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Char" /> object.</summary>
	[WhiteList("char.CompareTo(char)", WhiteListOp.Discard)]
	public extern static Number _309d33b86c3815d8(Number instance, Number value);

	///<summary>Converts the value of this instance to its equivalent string representation.</summary>
	[WhiteList("override char.ToString()", WhiteListOp.Discard)]
	public extern static string _4861ba21870a2ec3(Number instance);

	///<summary>Converts the value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("char.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _fc3c2436fe7b6197(Number instance, Intl.NumberFormat? provider);

	///<summary>Converts the specified Unicode character to its equivalent string representation.</summary>
	[WhiteList("static char.ToString(char)", WhiteListOp.Discard)]
	public extern static string _f59d4d8b2c441c53(Number c);

	///<summary>Converts the value of the specified string to its equivalent Unicode character.</summary>
	[WhiteList("static char.Parse(string)", WhiteListOp.Discard)]
	public extern static Number _d89999df761a6d2e(object s);

	///<summary>Converts the value of the specified string to its equivalent Unicode character. A return code indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static char.TryParse(string, out char)", WhiteListOp.Discard)]
	public extern static bool _9450f84427428db0(object s, Box<Number> result);

	///<summary>Indicates whether a character is categorized as an ASCII letter.</summary>
	[WhiteList("static char.IsAsciiLetter(char)", WhiteListOp.Discard)]
	public extern static bool _1737fc6cbaca1038(Number c);

	///<summary>Indicates whether a character is categorized as a lowercase ASCII letter.</summary>
	[WhiteList("static char.IsAsciiLetterLower(char)", WhiteListOp.Discard)]
	public extern static bool _d0f415a83ae10d8a(Number c);

	///<summary>Indicates whether a character is categorized as an uppercase ASCII letter.</summary>
	[WhiteList("static char.IsAsciiLetterUpper(char)", WhiteListOp.Discard)]
	public extern static bool _30f49ccd6f1f8b2d(Number c);

	///<summary>Indicates whether a character is categorized as an ASCII digit.</summary>
	[WhiteList("static char.IsAsciiDigit(char)", WhiteListOp.Discard)]
	public extern static bool _266ce5f0f0db2958(Number c);

	///<summary>Indicates whether a character is categorized as an ASCII letter or digit.</summary>
	[WhiteList("static char.IsAsciiLetterOrDigit(char)", WhiteListOp.Discard)]
	public extern static bool _3f3a99864b7042e9(Number c);

	///<summary>Indicates whether a character is categorized as an ASCII hexademical digit.</summary>
	[WhiteList("static char.IsAsciiHexDigit(char)", WhiteListOp.Discard)]
	public extern static bool _8ebed700a57241d2(Number c);

	///<summary>Indicates whether a character is categorized as an ASCII upper-case hexademical digit.</summary>
	[WhiteList("static char.IsAsciiHexDigitUpper(char)", WhiteListOp.Discard)]
	public extern static bool _47cc49555e21ab3b(Number c);

	///<summary>Indicates whether a character is categorized as an ASCII lower-case hexademical digit.</summary>
	[WhiteList("static char.IsAsciiHexDigitLower(char)", WhiteListOp.Discard)]
	public extern static bool _c082c46f951a0c9f(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as a decimal digit.</summary>
	[WhiteList("static char.IsDigit(char)", WhiteListOp.Discard)]
	public extern static bool _91a882221d295c32(Number c);

	///<summary>Indicates whether a character is within the specified inclusive range.</summary>
	[WhiteList("static char.IsBetween(char, char, char)", WhiteListOp.Discard)]
	public extern static bool _dfb76865a7840d43(Number c, Number minInclusive, Number maxInclusive);

	///<summary>Indicates whether the specified Unicode character is categorized as a Unicode letter.</summary>
	[WhiteList("static char.IsLetter(char)", WhiteListOp.Discard)]
	public extern static bool _38721338a529a8d7(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as white space.</summary>
	[WhiteList("static char.IsWhiteSpace(char)", WhiteListOp.Discard)]
	public extern static bool _16e351e6f7b127f7(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as an uppercase letter.</summary>
	[WhiteList("static char.IsUpper(char)", WhiteListOp.Discard)]
	public extern static bool _7d70d8021ab255a8(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as a lowercase letter.</summary>
	[WhiteList("static char.IsLower(char)", WhiteListOp.Discard)]
	public extern static bool _b344d14ce0e33570(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as a punctuation mark.</summary>
	[WhiteList("static char.IsPunctuation(char)", WhiteListOp.Discard)]
	public extern static bool _ce3de1c060963041(Number c);

	///<summary>Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.</summary>
	[WhiteList("static char.IsLetterOrDigit(char)", WhiteListOp.Discard)]
	public extern static bool _49432dd2165d98f0(Number c);

	///<summary>Converts the value of a specified Unicode character to its uppercase equivalent using specified culture-specific formatting information.</summary>
	[WhiteList("static char.ToUpper(char, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static Number _dd41639bb00c83ab(Number c, String culture);

	///<summary>Converts the value of a Unicode character to its uppercase equivalent.</summary>
	[WhiteList("static char.ToUpper(char)", WhiteListOp.Discard)]
	public extern static Number _2713512e6f5a9312(Number c);

	///<summary>Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant culture.</summary>
	[WhiteList("static char.ToUpperInvariant(char)", WhiteListOp.Discard)]
	public extern static Number _b0c91aa30cd2a5f7(Number c);

	///<summary>Converts the value of a specified Unicode character to its lowercase equivalent using specified culture-specific formatting information.</summary>
	[WhiteList("static char.ToLower(char, System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static Number _b81ddeb8c6240b72(Number c, String culture);

	///<summary>Converts the value of a Unicode character to its lowercase equivalent.</summary>
	[WhiteList("static char.ToLower(char)", WhiteListOp.Discard)]
	public extern static Number _b91d21a936e68017(Number c);

	///<summary>Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant culture.</summary>
	[WhiteList("static char.ToLowerInvariant(char)", WhiteListOp.Discard)]
	public extern static Number _76274ed9d45c0127(Number c);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Char" />.</summary>
	[WhiteList("char.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _84932c09c59d9b51(Number instance);

	///<summary>Indicates whether the specified Unicode character is categorized as a control character.</summary>
	[WhiteList("static char.IsControl(char)", WhiteListOp.Discard)]
	public extern static bool _c12d0a40e2ed8650(Number c);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a control character.</summary>
	[WhiteList("static char.IsControl(string, int)", WhiteListOp.Discard)]
	public extern static bool _68e189abbb5497dc(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a decimal digit.</summary>
	[WhiteList("static char.IsDigit(string, int)", WhiteListOp.Discard)]
	public extern static bool _52eb020022da112b(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a Unicode letter.</summary>
	[WhiteList("static char.IsLetter(string, int)", WhiteListOp.Discard)]
	public extern static bool _e7ee64c732d21cd5(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a letter or a decimal digit.</summary>
	[WhiteList("static char.IsLetterOrDigit(string, int)", WhiteListOp.Discard)]
	public extern static bool _d752ce4eaadf7612(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a lowercase letter.</summary>
	[WhiteList("static char.IsLower(string, int)", WhiteListOp.Discard)]
	public extern static bool _6ebe08db86ea37a2(object s, Number index);

	///<summary>Indicates whether the specified Unicode character is categorized as a number.</summary>
	[WhiteList("static char.IsNumber(char)", WhiteListOp.Discard)]
	public extern static bool _77e97c648607e65e(Number c);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a number.</summary>
	[WhiteList("static char.IsNumber(string, int)", WhiteListOp.Discard)]
	public extern static bool _5180e5acb1d4bcb0(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a punctuation mark.</summary>
	[WhiteList("static char.IsPunctuation(string, int)", WhiteListOp.Discard)]
	public extern static bool _5f7e394ed1d09372(object s, Number index);

	///<summary>Indicates whether the specified Unicode character is categorized as a separator character.</summary>
	[WhiteList("static char.IsSeparator(char)", WhiteListOp.Discard)]
	public extern static bool _066fc76a18dc824f(Number c);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a separator character.</summary>
	[WhiteList("static char.IsSeparator(string, int)", WhiteListOp.Discard)]
	public extern static bool _3d391ade47da71a6(object s, Number index);

	///<summary>Indicates whether the specified character has a surrogate code unit.</summary>
	[WhiteList("static char.IsSurrogate(char)", WhiteListOp.Discard)]
	public extern static bool _e5949fe4a1738a38(Number c);

	///<summary>Indicates whether the character at the specified position in a specified string has a surrogate code unit.</summary>
	[WhiteList("static char.IsSurrogate(string, int)", WhiteListOp.Discard)]
	public extern static bool _bca1b50c85e48723(object s, Number index);

	///<summary>Indicates whether the specified Unicode character is categorized as a symbol character.</summary>
	[WhiteList("static char.IsSymbol(char)", WhiteListOp.Discard)]
	public extern static bool _0f18b1b6d2524322(Number c);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as a symbol character.</summary>
	[WhiteList("static char.IsSymbol(string, int)", WhiteListOp.Discard)]
	public extern static bool _16587492d280e91d(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as an uppercase letter.</summary>
	[WhiteList("static char.IsUpper(string, int)", WhiteListOp.Discard)]
	public extern static bool _1ae24de44f4b499e(object s, Number index);

	///<summary>Indicates whether the character at the specified position in a specified string is categorized as white space.</summary>
	[WhiteList("static char.IsWhiteSpace(string, int)", WhiteListOp.Discard)]
	public extern static bool _a21dd6de62be7b75(object s, Number index);

	///<summary>Categorizes a specified Unicode character into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
	[WhiteList("static char.GetUnicodeCategory(char)", WhiteListOp.Discard)]
	public extern static System.Globalization.UnicodeCategory _226cc4ffd552fcf9(Number c);

	///<summary>Categorizes the character at the specified position in a specified string into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
	[WhiteList("static char.GetUnicodeCategory(string, int)", WhiteListOp.Discard)]
	public extern static System.Globalization.UnicodeCategory _e41ad686bd01aff1(object s, Number index);

	///<summary>Converts the specified numeric Unicode character to a double-precision floating point number.</summary>
	[WhiteList("static char.GetNumericValue(char)", WhiteListOp.Discard)]
	public extern static Number _d86c1e9964250116(Number c);

	///<summary>Converts the numeric Unicode character at the specified position in a specified string to a double-precision floating point number.</summary>
	[WhiteList("static char.GetNumericValue(string, int)", WhiteListOp.Discard)]
	public extern static Number _938251f1b1fc7bc8(object s, Number index);

	///<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a high surrogate.</summary>
	[WhiteList("static char.IsHighSurrogate(char)", WhiteListOp.Discard)]
	public extern static bool _4c066834beda061c(Number c);

	///<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a high surrogate.</summary>
	[WhiteList("static char.IsHighSurrogate(string, int)", WhiteListOp.Discard)]
	public extern static bool _311485d1745ce294(object s, Number index);

	///<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a low surrogate.</summary>
	[WhiteList("static char.IsLowSurrogate(char)", WhiteListOp.Discard)]
	public extern static bool _7761ca7b99042e8a(Number c);

	///<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a low surrogate.</summary>
	[WhiteList("static char.IsLowSurrogate(string, int)", WhiteListOp.Discard)]
	public extern static bool _1d56cdc9a261e948(object s, Number index);

	///<summary>Indicates whether two adjacent <see cref="T:System.Char" /> objects at a specified position in a string form a surrogate pair.</summary>
	[WhiteList("static char.IsSurrogatePair(string, int)", WhiteListOp.Discard)]
	public extern static bool _27c9fca9c829cc5e(object s, Number index);

	///<summary>Indicates whether the two specified <see cref="T:System.Char" /> objects form a surrogate pair.</summary>
	[WhiteList("static char.IsSurrogatePair(char, char)", WhiteListOp.Discard)]
	public extern static bool _efe9c9b601517069(Number highSurrogate, Number lowSurrogate);

	///<summary>Converts the specified Unicode code point into a UTF-16 encoded string.</summary>
	[WhiteList("static char.ConvertFromUtf32(int)", WhiteListOp.Discard)]
	public extern static string _fdcbb676a7d83aab(Number utf32);

	///<summary>Converts the value of a UTF-16 encoded surrogate pair into a Unicode code point.</summary>
	[WhiteList("static char.ConvertToUtf32(char, char)", WhiteListOp.Discard)]
	public extern static Number _f842e9b2f7fea133(Number highSurrogate, Number lowSurrogate);

	///<summary>Converts the value of a UTF-16 encoded character or surrogate pair at a specified position in a string into a Unicode code point.</summary>
	[WhiteList("static char.ConvertToUtf32(string, int)", WhiteListOp.Discard)]
	public extern static Number _d9f7c3c03ea64580(object s, Number index);
}
