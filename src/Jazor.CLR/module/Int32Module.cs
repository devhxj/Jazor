namespace Jazor.CLR;

/// <summary>
/// System.Int32 (int) 类型模块映射规则
///
/// C# int 与 JavaScript 的对应关系：
/// - C# int 是 32 位有符号整数
/// - JavaScript Number 是 64 位浮点数，但可以精确表示 32 位整数
/// - 使用 Math.floor、Math.ceil 等进行整数运算
///
/// Op 类型选择原则：
/// - Allowed: 操作符（+ - * / % == != &lt; &gt; &lt;= &gt;=）
/// - Inline: 简单比较和运算
/// - Replace: JS Math 方法
/// - Import: 需要完整实现的复杂逻辑（Parse/TryParse）
/// - Discard: 不支持的功能
/// </summary>
[ECMAScriptModule]
[Jazor(Op.Import, "int","System/Int32Module.js")]
public static class Int32Module
{
	/// <summary>
	/// C#: int.MaxValue
	/// JS: Number.MAX_SAFE_INTEGER (简化) 或 2147483647
	/// </summary>
	[Jazor(Op.Inline, "static int.MaxValue", "2147483647")]
	public extern static int _maxValue();

	/// <summary>
	/// C#: int.MinValue
	/// JS: -2147483648
	/// </summary>
	[Jazor(Op.Inline, "static int.MinValue", "-2147483648")]
	public extern static int _minValue();

	[Jazor(Op.Discard, "int.Int32()")]
	public extern static Number _d8bb920f83e7d97e();

	/// <summary>
	/// C#: int.BigMul(a, b)
	/// JS: BigInt(a) * BigInt(b)
	/// </summary>
	[Jazor(Op.Inline, "static int.BigMul(int, int)", "(BigInt(@#{0}) * BigInt(@#{1}))")]
	public extern static BigInt _6f2c27167c45a727(Number left, Number right);

	/// <summary>
	/// C#: int.CompareTo(obj)
	/// JS: instance - obj
	/// </summary>
	[Jazor(Op.Inline, "int.CompareTo(object)", "(@#{0} - (@#{1} ?? 0))")]
	public extern static Number _b03337a2a71c762d(Number instance, object? value);

	/// <summary>
	/// C#: int.CompareTo(value)
	/// JS: instance - value
	/// </summary>
	[Jazor(Op.Inline, "int.CompareTo(int)", "(@#{0} - @#{1})")]
	public extern static Number _741df6ab5c9e75bc(Number instance, Number value);

	/// <summary>
	/// C#: int.Equals(obj)
	/// JS: instance === obj
	/// </summary>
	[Jazor(Op.Inline, "override int.Equals(object)", "(@#{0} === @#{1})")]
	public extern static bool _3f3e17a78ac17712(Number instance, object? obj);

	/// <summary>
	/// C#: int.Equals(other)
	/// JS: instance === other
	/// </summary>
	[Jazor(Op.Inline, "int.Equals(int)", "(@#{0} === @#{1})")]
	public extern static bool _5e7fb3a45e5a8f45(Number instance, Number obj);

	[Jazor(Op.Discard, "override int.GetHashCode()")]
	public extern static Number _74e858272ce4a15a(Number instance);

	/// <summary>
	/// C#: int.ToString()
	/// JS: instance.toString()
	/// </summary>
	[Jazor(Op.Replace, "override int.ToString()", "toString")]
	public extern static string _0103494bc5e6253f(Number instance);

	[Jazor(Op.Discard, "int.ToString(string)")]
	public extern static string _2d79e025317a398b(Number instance, string? format);

	[Jazor(Op.Discard, "int.ToString(System.IFormatProvider)")]
	public extern static string _1c432a82e61a7193(Number instance, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "int.ToString(string, System.IFormatProvider)")]
	public extern static string _f57247af306a3082(Number instance, string? format, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "int.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _bf6eee9bbd850f13(Number instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "int.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _11b66442f91f5212(Number instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: int.Parse(s)
	/// JS: parseInt(s, 10) with validation
	/// </summary>
	[Jazor(Op.Import, "static int.Parse(string)")]
	public static Number _151ccc6045162f8f(string s)
	{
		if (s == null)
			throw new Error("ArgumentNullException: String cannot be null.");

		// 使用 C# int.Parse 验证
		return int.Parse(s);
	}

	[Jazor(Op.Discard, "static int.Parse(string, System.Globalization.NumberStyles)")]
	public extern static Number _976d6e5278dfc58f(string s, object style);

	[Jazor(Op.Discard, "static int.Parse(string, System.IFormatProvider)")]
	public extern static Number _bb24095a38bb9666(string s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static int.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _377c7ab241784b5b(string s, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static int.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _8791c7bfd3662e63(Uint32Array s, object style, Intl.NumberFormat? provider);

	/// <summary>
	/// C#: int.TryParse(s, out result)
	/// JS: 返回 [success, parsedValue]
	/// </summary>
	[Jazor(Op.Import, "static int.TryParse(string, out int)")]
	public static Array<object?> _16e2a901535b765e(string? s, Number result)
	{
		if (s == null)
			return [false, 0];

		if (int.TryParse(s, out int parsedValue))
			return [true, parsedValue];

		return [false, 0];
	}

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<char>, out int)")]
	public extern static Array<object?> _f6a664534980b0f4(Uint32Array s, Number result);

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<byte>, out int)")]
	public extern static Array<object?> _2acff5418dba43bd(Uint8Array utf8Text, Number result);

	[Jazor(Op.Discard, "static int.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out int)")]
	public extern static Array<object?> _69f925b0bfe7fa2a(string? s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out int)")]
	public extern static Array<object?> _b745c572061e8b30(Uint32Array s, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "int.GetTypeCode()")]
	public extern static System.TypeCode _5c5bca3bf690f9b1(Number instance);

	/// <summary>
	/// C#: int.DivRem(a, b)
	/// JS: [Math.floor(a / b), a % b]
	/// </summary>
	[Jazor(Op.Import, "static int.DivRem(int, int)")]
	public static (int Quotient, int Remainder) _d4cc9914e60e5643(Number left, Number right)
	{
		if (right == 0)
			throw new Error("DivideByZeroException");
		var quotient = (int)Math.Floor((double)(left / right));
		var remainder = (int)(left % right);
		return (quotient, remainder);
	}

	/// <summary>
	/// C#: int.LeadingZeroCount(value)
	/// JS: Math.clz32(value)
	/// </summary>
	[Jazor(Op.Replace, "static int.LeadingZeroCount(int)", "clz32")]
	public extern static Number _f4458d4939549cbc(Number value);

	/// <summary>
	/// C#: int.PopCount(value)
	/// JS: 使用位运算计算
	/// </summary>
	[Jazor(Op.Import, "static int.PopCount(int)")]
	public static Number _e04660fe6cb92bf1(Number value)
	{
		// 汉明权重算法
		uint v = (uint)(int)value;
		v = v - ((v >> 1) & 0x55555555);
		v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
		v = (v + (v >> 4)) & 0x0F0F0F0F;
		v = v + (v >> 8);
		v = v + (v >> 16);
		return (int)(v & 0x3F);
	}

	/// <summary>
	/// C#: int.RotateLeft(value, amount)
	/// JS: (value << amount) | (value >>> (32 - amount))
	/// </summary>
	[Jazor(Op.Inline, "static int.RotateLeft(int, int)", "((@#{0} << (@#{1} & 31)) | (@#{0} >>> (32 - (@#{1} & 31))))")]
	public extern static Number _f7913110e7d03a57(Number value, Number rotateAmount);

	/// <summary>
	/// C#: int.RotateRight(value, amount)
	/// JS: (value >>> amount) | (value << (32 - amount))
	/// </summary>
	[Jazor(Op.Inline, "static int.RotateRight(int, int)", "((@#{0} >>> (@#{1} & 31)) | (@#{0} << (32 - (@#{1} & 31))))")]
	public extern static Number _f090db0dba3c3b28(Number value, Number rotateAmount);

	/// <summary>
	/// C#: int.TrailingZeroCount(value)
	/// JS: 使用位运算
	/// </summary>
	[Jazor(Op.Import, "static int.TrailingZeroCount(int)")]
	public static Number _43a8a807a2b103c8(Number value)
	{
		if (value == 0)
			return 32;

		uint v = (uint)(int)value;
		int count = 0;
		while ((v & 1) == 0)
		{
			v >>= 1;
			count++;
		}
		return count;
	}

	/// <summary>
	/// C#: int.IsPow2(value)
	/// JS: value > 0 && (value & (value - 1)) === 0
	/// </summary>
	[Jazor(Op.Inline, "static int.IsPow2(int)", "(@#{0} > 0 &amp;&amp; (@#{0} &amp; (@#{0} - 1)) === 0)")]
	public extern static bool _8157179708f5a6c3(Number value);

	/// <summary>
	/// C#: int.Log2(value)
	/// JS: Math.floor(Math.log2(value))
	/// </summary>
	[Jazor(Op.Inline, "static int.Log2(int)", "Math.floor(Math.log2(@#{0}))")]
	public extern static Number _3173781f909bc9fc(Number value);

	/// <summary>
	/// C#: int.Clamp(value, min, max)
	/// JS: Math.min(Math.max(value, min), max)
	/// </summary>
	[Jazor(Op.Inline, "static int.Clamp(int, int, int)", "Math.min(Math.max(@#{0}, @#{1}), @#{2})")]
	public extern static Number _351e597bc27e1afc(Number value, Number min, Number max);

	/// <summary>
	/// C#: int.CopySign(value, sign)
	/// JS: sign >= 0 ? Math.abs(value) : -Math.abs(value)
	/// </summary>
	[Jazor(Op.Inline, "static int.CopySign(int, int)", "(@#{1} >= 0 ? Math.abs(@#{0}) : -Math.abs(@#{0}))")]
	public extern static Number _95793b26c4495935(Number value, Number sign);

	/// <summary>
	/// C#: int.Max(x, y)
	/// JS: Math.max(x, y)
	/// </summary>
	[Jazor(Op.Replace, "static int.Max(int, int)", "max")]
	public extern static Number _a98fdc6e84d091b3(Number x, Number y);

	/// <summary>
	/// C#: int.Min(x, y)
	/// JS: Math.min(x, y)
	/// </summary>
	[Jazor(Op.Replace, "static int.Min(int, int)", "min")]
	public extern static Number _a0b140070c2e6328(Number x, Number y);

	/// <summary>
	/// C#: int.Sign(value)
	/// JS: Math.sign(value)
	/// </summary>
	[Jazor(Op.Replace, "static int.Sign(int)", "sign")]
	public extern static Number _ab2e55d493adcdd8(Number value);

	/// <summary>
	/// C#: int.Abs(value)
	/// JS: Math.abs(value)
	/// </summary>
	[Jazor(Op.Replace, "static int.Abs(int)", "abs")]
	public extern static Number _49bf8261f5cf3a4b(Number value);

	[Jazor(Op.Discard, "static int.CreateChecked<TOther>(TOther)")]
	public extern static Number _275663af53fa5529<TOther>(object value);

	[Jazor(Op.Discard, "static int.CreateSaturating<TOther>(TOther)")]
	public extern static Number _570b24c0c63f26f9<TOther>(object value);

	[Jazor(Op.Discard, "static int.CreateTruncating<TOther>(TOther)")]
	public extern static Number _0315334a27eea649<TOther>(object value);

	/// <summary>
	/// C#: int.IsEvenInteger(value)
	/// JS: (value & 1) === 0
	/// </summary>
	[Jazor(Op.Inline, "static int.IsEvenInteger(int)", "((@#{0} &amp; 1) === 0)")]
	public extern static bool _4cbed0ce3a7f9c5f(Number value);

	/// <summary>
	/// C#: int.IsNegative(value)
	/// JS: value < 0
	/// </summary>
	[Jazor(Op.Inline, "static int.IsNegative(int)", "(@#{0} &lt; 0)")]
	public extern static bool _3d1db358d3f6d96f(Number value);

	/// <summary>
	/// C#: int.IsOddInteger(value)
	/// JS: (value & 1) !== 0
	/// </summary>
	[Jazor(Op.Inline, "static int.IsOddInteger(int)", "((@#{0} &amp; 1) !== 0)")]
	public extern static bool _0f92a85f87224c94(Number value);

	/// <summary>
	/// C#: int.IsPositive(value)
	/// JS: value > 0
	/// </summary>
	[Jazor(Op.Inline, "static int.IsPositive(int)", "(@#{0} &gt; 0)")]
	public extern static bool _280b1b013a39c514(Number value);

	/// <summary>
	/// C#: int.MaxMagnitude(x, y)
	/// JS: Math.abs(x) > Math.abs(y) ? x : y
	/// </summary>
	[Jazor(Op.Inline, "static int.MaxMagnitude(int, int)", "(Math.abs(@#{0}) &gt; Math.abs(@#{1}) ? @#{0} : @#{1})")]
	public extern static Number _a36b4a6dbd50fa77(Number x, Number y);

	/// <summary>
	/// C#: int.MinMagnitude(x, y)
	/// JS: Math.abs(x) < Math.abs(y) ? x : y
	/// </summary>
	[Jazor(Op.Inline, "static int.MinMagnitude(int, int)", "(Math.abs(@#{0}) &lt; Math.abs(@#{1}) ? @#{0} : @#{1})")]
	public extern static Number _d0c6a74fd11d24bf(Number x, Number y);

	[Jazor(Op.Discard, "static int.TryParse(string, System.IFormatProvider, out int)")]
	public extern static Array<object?> _a1335dcbd870906d(string? s, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static int.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Number _40d7b4fbe4ce5fc0(Uint32Array s, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out int)")]
	public extern static Array<object?> _635895827c275362(Uint32Array s, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static int.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static Number _a78d8d9d4b2f22f6(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out int)")]
	public extern static Array<object?> _e40b4c4d3f2f631c(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, Number result);

	[Jazor(Op.Discard, "static int.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static Number _835ae2f52c59c7ec(Uint8Array utf8Text, Intl.NumberFormat? provider);

	[Jazor(Op.Discard, "static int.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out int)")]
	public extern static Array<object?> _b1fd33b593bc8df8(Uint8Array utf8Text, Intl.NumberFormat? provider, Number result);
}
