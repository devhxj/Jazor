using System.Collections;
using ECMAScript.Common;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("int", WhiteListOp.Allowed, "System/Int32Module.js")]
public static class Int32Module
{
	//int.MaxValue = 2147483647;

	//int.MinValue = -2147483648;

	[WhiteList("int.Int32()", WhiteListOp.Discard)]
	public extern static Number _d8bb920f83e7d97e();

	[WhiteList("static int.Abs(int)", WhiteListOp.Discard)]
	public extern static Number _49bf8261f5cf3a4b(Number value);

	[WhiteList("static int.BigMul(int, int)", WhiteListOp.Discard)]
	public extern static BigInt _6f2c27167c45a727(Number left, Number right);

	[WhiteList("static int.Clamp(int, int, int)", WhiteListOp.Discard)]
	public extern static Number _351e597bc27e1afc(Number value, Number min, Number max);

	[WhiteList("int.CompareTo(int)", WhiteListOp.Discard)]
	public extern static Number _741df6ab5c9e75bc(Number instance, Number value);

	[WhiteList("int.CompareTo(object)", WhiteListOp.Discard)]
	public extern static Number _b03337a2a71c762d(Number instance, Object? value);

	[WhiteList("static int.CopySign(int, int)", WhiteListOp.Discard)]
	public extern static Number _95793b26c4495935(Number value, Number sign);

	[WhiteList("static int.CreateChecked<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _275663af53fa5529<TOther>(object value);

	[WhiteList("static int.CreateSaturating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _570b24c0c63f26f9<TOther>(object value);

	[WhiteList("static int.CreateTruncating<TOther>(TOther)", WhiteListOp.Discard)]
	public extern static Number _0315334a27eea649<TOther>(object value);

	[WhiteList("static int.DivRem(int, int)", WhiteListOp.Discard)]
	public extern static (int Quotient, int Remainder) _d4cc9914e60e5643(Number left, Number right);

	[WhiteList("int.Equals(int)", WhiteListOp.Discard)]
	public extern static bool _5e7fb3a45e5a8f45(Number instance, Number obj);

	[WhiteList("override int.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _3f3e17a78ac17712(Number instance, Object? obj);

	[WhiteList("override int.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _74e858272ce4a15a(Number instance);

	[WhiteList("int.GetTypeCode()", WhiteListOp.Discard)]
	public extern static System.TypeCode _5c5bca3bf690f9b1(Number instance);

	[WhiteList("static int.IsEvenInteger(int)", WhiteListOp.Discard)]
	public extern static bool _4cbed0ce3a7f9c5f(Number value);

	[WhiteList("static int.IsNegative(int)", WhiteListOp.Discard)]
	public extern static bool _3d1db358d3f6d96f(Number value);

	[WhiteList("static int.IsOddInteger(int)", WhiteListOp.Discard)]
	public extern static bool _0f92a85f87224c94(Number value);

	[WhiteList("static int.IsPositive(int)", WhiteListOp.Discard)]
	public extern static bool _280b1b013a39c514(Number value);

	[WhiteList("static int.IsPow2(int)", WhiteListOp.Discard)]
	public extern static bool _8157179708f5a6c3(Number value);

	[WhiteList("static int.LeadingZeroCount(int)", WhiteListOp.Discard)]
	public extern static Number _f4458d4939549cbc(Number value);

	[WhiteList("static int.Log2(int)", WhiteListOp.Discard)]
	public extern static Number _3173781f909bc9fc(Number value);

	[WhiteList("static int.Max(int, int)", WhiteListOp.Discard)]
	public extern static Number _a98fdc6e84d091b3(Number x, Number y);

	[WhiteList("static int.MaxMagnitude(int, int)", WhiteListOp.Discard)]
	public extern static Number _a36b4a6dbd50fa77(Number x, Number y);

	[WhiteList("static int.Min(int, int)", WhiteListOp.Discard)]
	public extern static Number _a0b140070c2e6328(Number x, Number y);

	[WhiteList("static int.MinMagnitude(int, int)", WhiteListOp.Discard)]
	public extern static Number _d0c6a74fd11d24bf(Number x, Number y);

	[WhiteList("static int.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _a78d8d9d4b2f22f6(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	[WhiteList("static int.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _835ae2f52c59c7ec(Uint8Array utf8Text, Intl.NumberFormat? provider);

	[WhiteList("static int.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _8791c7bfd3662e63(Uint32Array s, object style, Intl.NumberFormat? provider);

	[WhiteList("static int.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _40d7b4fbe4ce5fc0(Uint32Array s, Intl.NumberFormat? provider);

	[WhiteList("static int.Parse(string)", WhiteListOp.Discard)]
	public extern static Number _151ccc6045162f8f(object s);

	[WhiteList("static int.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static Number _976d6e5278dfc58f(object s, object style);

	[WhiteList("static int.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _377c7ab241784b5b(object s, object style, Intl.NumberFormat? provider);

	[WhiteList("static int.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static Number _bb24095a38bb9666(object s, Intl.NumberFormat? provider);

	[WhiteList("static int.PopCount(int)", WhiteListOp.Discard)]
	public extern static Number _e04660fe6cb92bf1(Number value);

	[WhiteList("static int.RotateLeft(int, int)", WhiteListOp.Discard)]
	public extern static Number _f7913110e7d03a57(Number value, Number rotateAmount);

	[WhiteList("static int.RotateRight(int, int)", WhiteListOp.Discard)]
	public extern static Number _f090db0dba3c3b28(Number value, Number rotateAmount);

	[WhiteList("static int.Sign(int)", WhiteListOp.Discard)]
	public extern static Number _ab2e55d493adcdd8(Number value);

	[WhiteList("override int.ToString()", WhiteListOp.Discard)]
	public extern static string _0103494bc5e6253f(Number instance);

	[WhiteList("int.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _1c432a82e61a7193(Number instance, Intl.NumberFormat? provider);

	[WhiteList("int.ToString(string)", WhiteListOp.Discard)]
	public extern static string _2d79e025317a398b(Number instance, object format);

	[WhiteList("int.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _f57247af306a3082(Number instance, object format, Intl.NumberFormat? provider);

	[WhiteList("static int.TrailingZeroCount(int)", WhiteListOp.Discard)]
	public extern static Number _43a8a807a2b103c8(Number value);

	[WhiteList("int.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _11b66442f91f5212(Number instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	[WhiteList("int.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _bf6eee9bbd850f13(Number instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _e40b4c4d3f2f631c(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _b1fd33b593bc8df8(Uint8Array utf8Text, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<byte>, out int)", WhiteListOp.Discard)]
	public extern static bool _2acff5418dba43bd(Uint8Array utf8Text, OutValue<Number> result);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _b745c572061e8b30(Uint32Array s, object style, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _635895827c275362(Uint32Array s, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(System.ReadOnlySpan<char>, out int)", WhiteListOp.Discard)]
	public extern static bool _f6a664534980b0f4(Uint32Array s, OutValue<Number> result);

	[WhiteList("static int.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _69f925b0bfe7fa2a(object s, object style, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(string, System.IFormatProvider, out int)", WhiteListOp.Discard)]
	public extern static bool _a1335dcbd870906d(string s, Intl.NumberFormat? provider, OutValue<Number> result);

	[WhiteList("static int.TryParse(string, out int)", WhiteListOp.Import)]
	public static bool _16e2a901535b765e(string s, OutValue<Number?> result)
	{
		result.Value = ParseInt(s);
		return result.Value == null || result.Value == Number.NaN;
	}
}
