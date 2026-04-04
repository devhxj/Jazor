namespace Jazor.CLR;

/// <summary>
/// System.Int16 (short) 类型模块映射规则
///
/// C# short 与 JavaScript 的对应关系：
/// - C# short 是 16 位有符号整数
/// - JavaScript Number 是 64 位浮点数，可以精确表示 16 位整数
///
/// Op 类型选择原则：
/// - Inline: 简单比较和运算
/// - Alias: JS 原生方法名替换
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule("System/Int16Module.js")]
[Jazor(Op.Alias, "short","Number")]
public static class Int16Module
{
	private static Number CompareCore(Number left, Number right)
		=> left < right ? -1 : (left > right ? 1 : 0);

	private static bool TryParseInt16Core(string? s, out Number value)
	{
		value = 0;
		if (s == null)
			return false;

		var trimmed = s.Trim();
		if (trimmed.Length == 0)
			return false;

		var start = 0;
		var first = trimmed[0];
		if (first == '+' || first == '-')
		{
			if (trimmed.Length == 1)
				return false;

			start = 1;
		}

		for (var i = start; i < trimmed.Length; i++)
		{
			var ch = trimmed[i];
			if (ch < '0' || ch > '9')
				return false;
		}

		var parsed = Number_(trimmed);
		if (IsNaN(parsed) || Math.Floor_(parsed) != parsed)
			return false;
		if (parsed < -32768 || parsed > 32767)
			return false;

		value = parsed;
		return true;
	}

	private static Number MaxMagnitudeCore(Number x, Number y)
	{
		var absX = Math.Abs_(x);
		var absY = Math.Abs_(y);
		if (absX > absY)
			return x;
		if (absX < absY)
			return y;

		return CompareCore(x, y) >= 0 ? x : y;
	}

	private static Number MinMagnitudeCore(Number x, Number y)
	{
		var absX = Math.Abs_(x);
		var absY = Math.Abs_(y);
		if (absX < absY)
			return x;
		if (absX > absY)
			return y;

		return CompareCore(x, y) <= 0 ? x : y;
	}

	/// <summary>
	/// C#: short.MaxValue
	/// JS: 32767
	/// </summary>
	[Jazor(Op.Inline, "static short.MaxValue", "32767")]
	public extern static Number _maxValue();

	/// <summary>
	/// C#: short.MinValue
	/// JS: -32768
	/// </summary>
	[Jazor(Op.Inline, "static short.MinValue", "-32768")]
	public extern static Number _minValue();

	[Jazor(Op.Discard, "short.Int16()")]
	public extern static Number _562bb08ad63be5d7();

	/// <summary>
	/// C#: short.CompareTo(object)
	/// JS: 与 .NET 一致的 CompareTo 规则，单独处理 null 和类型检查
	/// </summary>
	[Jazor(Op.Import, "short.CompareTo(object)")]
	public static Number _16417ddcfd71e8e5(Number instance, object? value)
	{
		if (value == null)
			return 1;
		if (TypeOf(value) != "number")
			throw new Error("ArgumentException: Object must be of type Int16.");

		return CompareCore(instance, (Number)value);
	}

	/// <summary>
	/// C#: short.CompareTo(short)
	/// JS: 返回负数、零或正数
	/// </summary>
	[Jazor(Op.Inline, "short.CompareTo(short)", "(__arg1 < __arg2 ? -1 : (__arg1 > __arg2 ? 1 : 0))")]
	public extern static Number _4ee8d8c1e1a45502(Number instance, Number value);

	/// <summary>
	/// C#: short.Equals(object)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override short.Equals(object)", "(__arg1 === __arg2)")]
	public extern static bool _22027e397eeeadf4(Number instance, object? obj);

	/// <summary>
	/// C#: short.Equals(short)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "short.Equals(short)", "(__arg1 === __arg2)")]
	public extern static bool _cc018b8cb5a7c74c(Number instance, Number obj);

	[Jazor(Op.Discard, "override short.GetHashCode()")]
	public extern static Number _b813268a9990cfbe(Number instance);

	/// <summary>
	/// C#: short.ToString()
	/// JS: instance.toString()
	/// </summary>
	[Jazor(Op.Alias, "override short.ToString()", "toString")]
	public extern static string _300da933adcd7412(Number instance);

	[Jazor(Op.Discard, "short.ToString(System.IFormatProvider)")]
	public extern static string _46ad91354004146c(Number instance, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "short.ToString(string)")]
	public extern static string _700b60c63bd82c5d(Number instance, string? format);

	[Jazor(Op.Discard, "short.ToString(string, System.IFormatProvider)")]
	public extern static string _ffb38f7355a8b434(Number instance, string? format, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "short.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _545cfe8d9fec0470(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "short.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _cf56eee1e0199bff(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: short.Parse(s)
	/// JS: 只接受十进制整数字符串，拒绝尾随垃圾字符
	/// </summary>
	[Jazor(Op.Import, "static short.Parse(string)")]
	public static Number _8a975b9eda8ac957(string? s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");
		if (!TryParseInt16Core(s, out var value))
			throw new Error($"FormatException: String '{s}' was not recognized as a valid Int16.");

		return value;
	}

	[Jazor(Op.Discard, "static short.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _64bcec0f7b8ae902(string s, object style);

	[Jazor(Op.Discard, "static short.Parse(string, System.IFormatProvider)")]
	public extern static Number _4f63dd7e755ab151(string s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static short.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _8457b89fab66282c(string s, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static short.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _c23c80430bf1bf6a(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: short.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static short.TryParse(string, out short)")]
	public static Array<object?> _65bc2566851a5ef7(string? s, Number result)
	{
		if (!TryParseInt16Core(s, out var value))
			return [false, 0];

		return [true, value];
	}

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<char>, out short)")]
	public extern static Array<object?> _f06bf367c8a26691(Uint32Array s, Number result);

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<byte>, out short)")]
	public extern static Array<object?> _af732a8ac69b6f6e(Uint8Array utf8Text, Number result);

	[Jazor(Op.Discard, "static short.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _cb5aaf07104e3199(string? s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _74bca5547a182d94(Uint32Array s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Inline, "short.GetTypeCode()", "7")]
	public extern static System.TypeCode _40232ebb0dcadbf1(Number instance);

	/// <summary>
	/// C#: short.DivRem(left, right)
	/// JS: 商按 .NET 整数除法语义向零截断
	/// </summary>
	[Jazor(Op.Import, "static short.DivRem(short, short)")]
	public static (short Quotient, short Remainder) _b2c1f15fae072110(Number left, Number right)
	{
		if (right == 0)
			throw new Error("DivideByZeroException");
		var quotient = Math.Trunc_(left / right);
		var remainder = left - quotient * right;
		return ((short)quotient, (short)remainder);
	}

	[Jazor(Op.Inline, "static short.LeadingZeroCount(short)", "(__arg1 === 0 ? 16 : (Math.clz32((__arg1 & 0xFFFF)) - 16))")]
	public extern static Number _52aba2834bccd915(Number value);

	/// <summary>
	/// C#: short.PopCount(value)
	/// JS: 先归一到 16 位补码，再使用汉明权重算法
	/// </summary>
	[Jazor(Op.Import, "static short.PopCount(short)")]
	public static Number _1636c956519f95fa(Number value)
	{
		uint v = (ushort)(short)value;
		v = v - ((v >> 1) & 0x5555);
		v = (v & 0x3333) + ((v >> 2) & 0x3333);
		v = (v + (v >> 4)) & 0x0F0F;
		v = v + (v >> 8);
		return (int)(v & 0x1F);
	}

	[Jazor(Op.Inline, "static short.RotateLeft(short, int)", "(((((__arg1 & 0xFFFF) << (__arg2 & 15)) | ((__arg1 & 0xFFFF) >>> (16 - (__arg2 & 15)))) & 0xFFFF) << 16 >> 16)")]
	public extern static Number _bae87098d1a8d51f(Number value, Number rotateAmount);

	[Jazor(Op.Inline, "static short.RotateRight(short, int)", "(((((__arg1 & 0xFFFF) >>> (__arg2 & 15)) | ((__arg1 & 0xFFFF) << (16 - (__arg2 & 15)))) & 0xFFFF) << 16 >> 16)")]
	public extern static Number _9d0ea1985ea5d86c(Number value, Number rotateAmount);

	[Jazor(Op.Inline, "static short.TrailingZeroCount(short)", "(__arg1 === 0 ? 16 : Math.floor(Math.log2(((__arg1 & 0xFFFF) & (-(__arg1 & 0xFFFF))))))")]
	public extern static Number _34f7d9d508f3d3fa(Number value);

	/// <summary>
	/// C#: short.IsPow2(value)
	/// JS: value > 0 && (value & (value - 1)) === 0
	/// </summary>
	[Jazor(Op.Inline, "static short.IsPow2(short)", "(__arg1 > 0 && (__arg1 & (__arg1 - 1)) === 0)")]
	public extern static bool _7f2d59a3c443c4ad(Number value);

	/// <summary>
	/// C#: short.Log2(value)
	/// JS: Math.floor(Math.log2(value))
	/// </summary>
	[Jazor(Op.Inline, "static short.Log2(short)", "Math.floor(Math.log2(__arg1))")]
	public extern static Number _35f45babf0c06295(Number value);

	/// <summary>
	/// C#: short.Clamp(value, min, max)
	/// JS: Math.min(Math.max(value, min), max)
	/// </summary>
	[Jazor(Op.Inline, "static short.Clamp(short, short, short)", "Math.min(Math.max(__arg1, __arg2), __arg3)")]
	public extern static Number _ab81977f8ce898b6(Number value, Number min, Number max);

	/// <summary>
	/// C#: short.CopySign(value, sign)
	/// JS: short 不存在 -0，可稳定收敛为单表达式。
	/// </summary>
	[Jazor(Op.Inline, "static short.CopySign(short, short)", "(__arg2 < 0 ? -Math.abs(__arg1) : Math.abs(__arg1))")]
	public extern static Number _84dbfd61502b67c2(Number value, Number sign);

	/// <summary>
	/// C#: short.Max(x, y)
	/// JS: Math.max(x, y)
	/// </summary>
	[Jazor(Op.Inline, "static short.Max(short, short)", "Math.max(__arg1, __arg2)")]
	public extern static Number _3373f84658d4d175(Number x, Number y);

	/// <summary>
	/// C#: short.Min(x, y)
	/// JS: Math.min(x, y)
	/// </summary>
	[Jazor(Op.Inline, "static short.Min(short, short)", "Math.min(__arg1, __arg2)")]
	public extern static Number _02506ba99181e464(Number x, Number y);

	/// <summary>
	/// C#: short.Sign(value)
	/// JS: value > 0 ? 1 : (value < 0 ? -1 : 0)
	/// </summary>
	[Jazor(Op.Inline, "static short.Sign(short)", "(__arg1 > 0 ? 1 : (__arg1 < 0 ? -1 : 0))")]
	public extern static Number _566e8c96791a4a93(Number value);

	/// <summary>
	/// C#: short.Abs(value)
	/// JS: Math.abs(value)
	/// </summary>
	[Jazor(Op.Inline, "static short.Abs(short)", "Math.abs(__arg1)")]
	public extern static Number _8ce36b36c4abd947(Number value);

	[Jazor(Op.Discard, "static short.CreateChecked<TOther>(TOther)")]
	public extern static Number _5fc26fbc77170159<TOther>(object value);

	[Jazor(Op.Discard, "static short.CreateSaturating<TOther>(TOther)")]
	public extern static Number _0803cae0198e4e4a<TOther>(object value);

	[Jazor(Op.Discard, "static short.CreateTruncating<TOther>(TOther)")]
	public extern static Number _4da6b11d651bbbb0<TOther>(object value);

	/// <summary>
	/// C#: short.IsEvenInteger(value)
	/// JS: (value & 1) === 0
	/// </summary>
	[Jazor(Op.Inline, "static short.IsEvenInteger(short)", "((__arg1 & 1) === 0)")]
	public extern static bool _316df8d3092665d2(Number value);

	/// <summary>
	/// C#: short.IsNegative(value)
	/// JS: value < 0
	/// </summary>
	[Jazor(Op.Inline, "static short.IsNegative(short)", "(__arg1 < 0)")]
	public extern static bool _1d7ab190b3eef427(Number value);

	/// <summary>
	/// C#: short.IsOddInteger(value)
	/// JS: (value & 1) !== 0
	/// </summary>
	[Jazor(Op.Inline, "static short.IsOddInteger(short)", "((__arg1 & 1) !== 0)")]
	public extern static bool _e35c3640561ad6e4(Number value);

	/// <summary>
	/// C#: short.IsPositive(value)
	/// JS: value > 0
	/// </summary>
	[Jazor(Op.Inline, "static short.IsPositive(short)", "(__arg1 > 0)")]
	public extern static bool _f65c31648c1c40d7(Number value);

	/// <summary>
	/// C#: short.MaxMagnitude(x, y)
	/// JS: 先比较绝对值；相同绝对值时按数值大小决胜。
	/// </summary>
	[Jazor(Op.Import, "static short.MaxMagnitude(short, short)")]
	public static Number _ea75510d32bc8099(Number x, Number y)
		=> MaxMagnitudeCore(x, y);

	/// <summary>
	/// C#: short.MinMagnitude(x, y)
	/// JS: 先比较绝对值；相同绝对值时按数值大小决胜。
	/// </summary>
	[Jazor(Op.Import, "static short.MinMagnitude(short, short)")]
	public static Number _63d3d54252a49e29(Number x, Number y)
		=> MinMagnitudeCore(x, y);

	[Jazor(Op.Discard, "static short.TryParse(string, System.IFormatProvider, out short)")]
	public extern static Array<object?> _1726573b3ed2620b(string? s, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static short.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _68a3295a7ebacac9(Uint32Array s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out short)")]
	public extern static Array<object?> _5849d879c5ca8c59(Uint32Array s, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static short.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _0795bea51a359cfe(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out short)")]
	public extern static Array<object?> _c09ca931ddd2f2ca(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static short.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _50cc08bf7c6985cb(Uint8Array utf8Text, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static short.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out short)")]
	public extern static Array<object?> _91d5e9e62716bef1(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
