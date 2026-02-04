using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("decimal", WhiteListOp.Allowed, null, "System/DecimalModule.js")]
public static class DecimalModule
{
	//decimal.Zero = 0;
	//decimal.One = 1;
	//decimal.MinusOne = -1;
	//decimal.MaxValue = 79228162514264337593543950335;
	//decimal.MinValue = -79228162514264337593543950335;

	[WhiteList("decimal.Decimal()", WhiteListOp.Discard)]
	public extern static String _a7246904c5449b5f();

	///<summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
	[WhiteList("decimal.CompareTo(object)", WhiteListOp.CompareTo)]
	public extern static Number _ff0e77ab6566e092(String instance, Object? value);

	///<summary>Compares this instance to a specified <see cref="T:System.Decimal" /> object and returns a comparison of their relative values.</summary>
	[WhiteList("decimal.CompareTo(decimal)", WhiteListOp.CompareTo)]
	public extern static Number _ca8a78810233056c(String instance, String value);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Object" /> represent the same type and value.</summary>
	[WhiteList("override decimal.Equals(object)", WhiteListOp.Equals)]
	public extern static bool _8abe47785e51f122(String instance, Object? value);

	///<summary>Returns a value indicating whether this instance and a specified <see cref="T:System.Decimal" /> object represent the same value.</summary>
	[WhiteList("decimal.Equals(decimal)", WhiteListOp.Equals)]
	public extern static bool _3dfd87d9d2f35e11(String instance, String value);

	///<summary>Returns the hash code for this instance.</summary>
	[WhiteList("override decimal.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _f58659c33299d2b1(String instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
	[WhiteList("override decimal.ToString()", WhiteListOp.Replace, "toString")]
	public extern static string _65a0e4fe8ccdd829(String instance);

	///<summary>Converts the numeric value of this instance to its equivalent string representation, using the specified format.</summary>
	[WhiteList("decimal.ToString(string)", WhiteListOp.Discard)]
	public extern static string _af32d07083f1da07(String instance, object format);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
	[WhiteList("decimal.ToString(System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _6234ba988b3e006d(String instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[WhiteList("decimal.ToString(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static string _b1e6a06111674f0c(String instance, object format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
	[WhiteList("decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _919259e7087cfd17(String instance, Uint32Array destination, Box<Number> charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[WhiteList("decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static bool _c5d11df37776e790(String instance, Uint8Array utf8Destination, Box<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[WhiteList("static decimal.Parse(string)", WhiteListOp.Import)]
	public static Number _91a2436283a24315(string s)
	{
		return Number(s);
	}

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[WhiteList("static decimal.Parse(string, System.Globalization.NumberStyles)", WhiteListOp.Discard)]
	public extern static String _79a0e8ede29256cc(object s, object style);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
	[WhiteList("static decimal.Parse(string, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static String _01be2a34fe2cda4e(object s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number in a specified style and culture-specific format to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[WhiteList("static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static String _f525a420b2d600ec(object s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[WhiteList("static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)", WhiteListOp.Discard)]
	public extern static String _8e0c949ee2411c7f(Uint32Array s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static decimal.TryParse(string, out decimal)", WhiteListOp.Import)]
	public static bool _e96278809bb50e35(string s, Box<Number> result)
	{
		try
		{
			result.Value = Number(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)", WhiteListOp.Discard)]
	public extern static bool _5f6432cf52162431(Uint32Array s, Box<String> result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its signed decimal equivalent.</summary>
	[WhiteList("static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)", WhiteListOp.Discard)]
	public extern static bool _0111d7c27998205b(Uint8Array utf8Text, Box<String> result);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)", WhiteListOp.Discard)]
	public extern static bool _b4ecd2424c9a371e(object s, object style, Intl.NumberFormat? provider, Box<String> result);

	///<summary>Converts the span representation of a number in a specified style and culture-specific format to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[WhiteList("static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)", WhiteListOp.Discard)]
	public extern static bool _ed6b24306e2ef5cd(Uint32Array s, object style, Intl.NumberFormat? provider, Box<String> result);

	// ... 所有其他方法保持 Discard 状态
}
