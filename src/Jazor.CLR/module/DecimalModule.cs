namespace Jazor.CLR;

[ECMAScriptModule("System/DecimalModule.js")]
[Jazor(Op.Alias, "decimal","String")]
public static class DecimalModule
{
	//decimal.Zero = 0;

	//decimal.One = 1;

	//decimal.MinusOne = -1;

	//decimal.MaxValue = 79228162514264337593543950335;

	//decimal.MinValue = -79228162514264337593543950335;

	/// <summary>
	/// C#: new decimal()
	/// JS: "0" (decimal as string for precision)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal()", "'0'")]
	public extern static string _a7246904c5449b5f();

	/// <summary>
	/// C#: new decimal(int)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(int)", "String(@#{0})")]
	public extern static string _9c4dd6829012e347(Number value);

	/// <summary>
	/// C#: new decimal(uint)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(uint)", "String(@#{0})")]
	public extern static string _73a058b17ed5de01(Number value);

	/// <summary>
	/// C#: new decimal(long)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(long)", "String(@#{0})")]
	public extern static string _188ee93a8a80b7f4(BigInt value);

	/// <summary>
	/// C#: new decimal(ulong)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(ulong)", "String(@#{0})")]
	public extern static string _9a3a0f6f89e1e594(BigInt value);

	/// <summary>
	/// C#: new decimal(float)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(float)", "String(@#{0})")]
	public extern static string _2f7f0d9035a4bbf6(Number value);

	/// <summary>
	/// C#: new decimal(double)
	/// JS: String(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Decimal(double)", "String(@#{0})")]
	public extern static string _cb7c7a937d3b8460(Number value);

	///<summary>Converts the specified 64-bit signed integer, which contains an OLE Automation Currency value, to the equivalent <see cref="T:System.Decimal" /> value.</summary>
	[Jazor(Op.Discard ,"static decimal.FromOACurrency(long)")]
	public extern static string _6cd0f8dfbedd7209(BigInt cy);

	///<summary>Converts the specified <see cref="T:System.Decimal" /> value to the equivalent OLE Automation Currency value, which is contained in a 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToOACurrency(decimal)")]
	public extern static BigInt _5d257b5cc33cdaeb(string value);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in a specified array.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(int[])")]
	public extern static string _1189e4d3b4884066(object bits);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> to a decimal value represented in binary and contained in the specified span.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(System.ReadOnlySpan<int>)")]
	public extern static string _e195522f8f6783c0(object bits);

	///<summary>Initializes a new instance of <see cref="T:System.Decimal" /> from parameters specifying the instance's constituent parts.</summary>
	[Jazor(Op.Discard ,"decimal.Decimal(int, int, int, bool, byte)")]
	public extern static string _030063a806322293(Number lo, Number mid, Number hi, bool isNegative, Number scale);

	[Jazor(Op.Discard ,"decimal.Scale.get")]
	public extern static Number _db7e7c8def75fee8(string instance);

	/// <summary>
	/// C#: decimal.Add(d1, d2)
	/// JS: String(Number(d1) + Number(d2))
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Add(decimal, decimal)", "String(Number(@#{0}) + Number(@#{1}))")]
	public extern static string _f73258f14e05c790(string d1, string d2);

	/// <summary>
	/// C#: decimal.Ceiling(d)
	/// JS: String(Math.ceil(Number(d)))
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Ceiling(decimal)", "String(Math.ceil(Number(@#{0})))")]
	public extern static string _84028a6e79626057(string d);

	/// <summary>
	/// C#: decimal.Compare(d1, d2)
	/// JS: Number(d1) < Number(d2) ? -1 : (Number(d1) > Number(d2) ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Compare(decimal, decimal)", "(Number(@#{0}) < Number(@#{1}) ? -1 : (Number(@#{0}) > Number(@#{1}) ? 1 : 0))")]
	public extern static Number _c11e0aef6b5ccf1e(string d1, string d2);

	///<summary>Compares this instance to a specified object and returns a comparison of their relative values.</summary>
	[Jazor(Op.Discard ,"decimal.CompareTo(object)")]
	public extern static Number _ff0e77ab6566e092(string instance, object? value);

	/// <summary>
	/// C#: instance.CompareTo(value)
	/// JS: Number(instance) < Number(value) ? -1 : (Number(instance) > Number(value) ? 1 : 0)
	/// </summary>
	[Jazor(Op.Inline, "decimal.CompareTo(decimal)", "(Number(@#{0}) < Number(@#{1}) ? -1 : (Number(@#{0}) > Number(@#{1}) ? 1 : 0))")]
	public extern static Number _ca8a78810233056c(string instance, string value);

	/// <summary>
	/// C#: decimal.Divide(d1, d2)
	/// JS: String(Number(d1) / Number(d2))
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Divide(decimal, decimal)", "String(Number(@#{0}) / Number(@#{1}))")]
	public extern static string _f5c1c0a2a040b000(string d1, string d2);

	/// <summary>
	/// C#: instance.Equals(value)
	/// JS: Number(instance) === Number(value)
	/// </summary>
	[Jazor(Op.Inline, "override decimal.Equals(object)", "(typeof @#{1} === 'string' && Number(@#{0}) === Number(@#{1}))")]
	public extern static bool _8abe47785e51f122(string instance, object? value);

	/// <summary>
	/// C#: instance.Equals(value)
	/// JS: Number(instance) === Number(value)
	/// </summary>
	[Jazor(Op.Inline, "decimal.Equals(decimal)", "(Number(@#{0}) === Number(@#{1}))")]
	public extern static bool _3dfd87d9d2f35e11(string instance, string value);

	/// <summary>
	/// C#: instance.GetHashCode()
	/// JS: Number(instance) | 0 (convert to int32)
	/// </summary>
	[Jazor(Op.Inline, "override decimal.GetHashCode()", "(Number(@#{0}) | 0)")]
	public extern static Number _f58659c33299d2b1(string instance);

	/// <summary>
	/// C#: decimal.Equals(d1, d2)
	/// JS: Number(d1) === Number(d2)
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Equals(decimal, decimal)", "(Number(@#{0}) === Number(@#{1}))")]
	public extern static bool _b25c4446c28ed255(string d1, string d2);

	/// <summary>
	/// C#: decimal.Floor(d)
	/// JS: String(Math.floor(Number(d)))
	/// </summary>
	[Jazor(Op.Inline, "static decimal.Floor(decimal)", "String(Math.floor(Number(@#{0})))")]
	public extern static string _518facaaeeb29ead(string d);

	/// <summary>
	/// C#: instance.ToString()
	/// JS: instance
	/// </summary>
	[Jazor(Op.Inline, "override decimal.ToString()", "@#{0}")]
	public extern static string _65a0e4fe8ccdd829(string instance);

	/// <summary>
	/// C#: instance.ToString(format)
	/// JS: Number(instance).toFixed(Number(format.replace(/[^0-9]/g, '')) || 0)
	/// </summary>
	[Jazor(Op.Import, "decimal.ToString(string)")]
	public static string _af32d07083f1da07(string instance, string? format)
	{
		if (format == null || format.Length == 0)
			return instance;
		// 简单格式化处理
		var num = Double.Parse(instance);
		return num.ToString(format);
	}

	/// <summary>
	/// C#: instance.ToString(provider)
	/// JS: instance
	/// </summary>
	[Jazor(Op.Inline, "decimal.ToString(System.IFormatProvider)", "@#{0}")]
	public extern static string _6234ba988b3e006d(string instance, Intl.NumberFormat? provider);

	///<summary>Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.</summary>
	[Jazor(Op.Discard ,"decimal.ToString(string, System.IFormatProvider)")]
	public extern static string _b1e6a06111674f0c(string instance, string? format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current decimal instance into the provided span of characters.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _919259e7087cfd17(string instance, Uint32Array destination, Number charsWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
	[Jazor(Op.Discard ,"decimal.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static Array<object?> _c5d11df37776e790(string instance, Uint8Array utf8Destination, Number bytesWritten, Uint32Array format, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(string)")]
	public extern static string _91a2436283a24315(string s);

	///<summary>Converts the string representation of a number in a specified style to its <see cref="T:System.Decimal" /> equivalent.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(string, System.Globalization.NumberStyles)")]
	public extern static string _79a0e8ede29256cc(string s, object style);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified culture-specific format information.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(string, System.IFormatProvider)")]
	public extern static string _01be2a34fe2cda4e(string s, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static string _f525a420b2d600ec(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static string _8e0c949ee2411c7f(string s, object style, Intl.NumberFormat? provider);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(string, out decimal)")]
	public extern static Array<object?> _e96278809bb50e35(string? s, string result);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)")]
	public extern static Array<object?> _5f6432cf52162431(string s, string result);

	///<summary>Tries to convert a UTF-8 character span containing the string representation of a number to its signed decimal equivalent.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)")]
	public extern static Array<object?> _0111d7c27998205b(Uint8Array utf8Text, string result);

	///<summary>Converts the string representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _b4ecd2424c9a371e(string? s, object style, Intl.NumberFormat? provider, string result);

	///<summary>Converts the span representation of a number to its <see cref="T:System.Decimal" /> equivalent using the specified style and culture-specific format. A return value indicates whether the conversion succeeded or failed.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _ed6b24306e2ef5cd(string s, object style, Intl.NumberFormat? provider, string result);

	///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.GetBits(decimal)")]
	public extern static int[] _e0536acf9668ef57(string d);

	///<summary>Converts the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.GetBits(decimal, System.Span<int>)")]
	public extern static Number _9d53437d519e15cb(string d, object destination);

	///<summary>Tries to convert the value of a specified instance of <see cref="T:System.Decimal" /> to its equivalent binary representation.</summary>
	[Jazor(Op.Discard ,"static decimal.TryGetBits(decimal, System.Span<int>, out int)")]
	public extern static Array<object?> _db7a1f9648d8e6eb(string d, object destination, Number valuesWritten);

	///<summary>Computes the remainder after dividing two <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Discard ,"static decimal.Remainder(decimal, decimal)")]
	public extern static string _700359e0de148ee3(string d1, string d2);

	///<summary>Multiplies two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Discard ,"static decimal.Multiply(decimal, decimal)")]
	public extern static string _d5be5da3d4effe96(string d1, string d2);

	///<summary>Returns the result of multiplying the specified <see cref="T:System.Decimal" /> value by negative one.</summary>
	[Jazor(Op.Discard ,"static decimal.Negate(decimal)")]
	public extern static string _26945a698afa2a91(string d);

	///<summary>Rounds a decimal value to the nearest integer.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal)")]
	public extern static string _4a816369b59f1ca3(string d);

	///<summary>Rounds a <see cref="T:System.Decimal" /> value to a specified number of decimal places.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, int)")]
	public extern static string _bc3a974d51c694ab(string d, Number decimals);

	///<summary>Rounds a decimal value to an integer using the specified rounding strategy.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, System.MidpointRounding)")]
	public extern static string _a334f7e82122cfc2(string d, object mode);

	///<summary>Rounds a decimal value to the specified precision using the specified rounding strategy.</summary>
	[Jazor(Op.Discard ,"static decimal.Round(decimal, int, System.MidpointRounding)")]
	public extern static string _09ee3a4652dbe73c(string d, Number decimals, object mode);

	///<summary>Subtracts a specified <see cref="T:System.Decimal" /> value from another.</summary>
	[Jazor(Op.Discard ,"static decimal.Subtract(decimal, decimal)")]
	public extern static string _3e80f2d9cf753d05(string d1, string d2);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToByte(decimal)")]
	public extern static Number _d2aabede7e0207c1(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 8-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToSByte(decimal)")]
	public extern static Number _175bf5ee849fcf8f(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt16(decimal)")]
	public extern static Number _5df8c6a064c50c5f(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent double-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.ToDouble(decimal)")]
	public extern static Number _cfbbd251b43c99f4(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt32(decimal)")]
	public extern static Number _ad71e0d1a8679244(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToInt64(decimal)")]
	public extern static BigInt _7a077e2e1baba462(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 16-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt16(decimal)")]
	public extern static Number _21bc553743dd324b(string value);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 32-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt32(decimal)")]
	public extern static Number _c975b2e5b2f4c009(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent 64-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.ToUInt64(decimal)")]
	public extern static BigInt _9b15def492d41a4a(string d);

	///<summary>Converts the value of the specified <see cref="T:System.Decimal" /> to the equivalent single-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.ToSingle(decimal)")]
	public extern static Number _1450e4ab34b1a945(string d);

	///<summary>Returns the integral digits of the specified <see cref="T:System.Decimal" />; any fractional digits are discarded.</summary>
	[Jazor(Op.Discard ,"static decimal.Truncate(decimal)")]
	public extern static string _be8b149ea0e1d76b(string d);

	///<summary>Defines an implicit conversion of an 8-bit unsigned integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(byte)")]
	public extern static string _c605c67b2cd1973c();

	///<summary>Defines an implicit conversion of an 8-bit signed integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(sbyte)")]
	public extern static string _e8d5240b7aa52784();

	///<summary>Defines an implicit conversion of a 16-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(short)")]
	public extern static string _8635fe57a74e1249();

	///<summary>Defines an implicit conversion of a 16-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(ushort)")]
	public extern static string _7c3cfa0de18bd43c();

	///<summary>Defines an implicit conversion of a Unicode character to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(char)")]
	public extern static string _d4af042bf014fd51();

	///<summary>Defines an implicit conversion of a 32-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(int)")]
	public extern static string _f5a5d600ccd38777();

	///<summary>Defines an implicit conversion of a 32-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(uint)")]
	public extern static string _d8b659cd861d2409();

	///<summary>Defines an implicit conversion of a 64-bit signed integer to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(long)")]
	public extern static string _23103e069358ca06();

	///<summary>Defines an implicit conversion of a 64-bit unsigned integer to a <see cref="T:System.Decimal" />. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.implicit operator decimal(ulong)")]
	public extern static string _7ab8c627f74cb718();

	///<summary>Defines an explicit conversion of a single-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator decimal(float)")]
	public extern static string _f456cac2ae523add();

	///<summary>Defines an explicit conversion of a double-precision floating-point number to a <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator decimal(double)")]
	public extern static string _8f3a66f6dc828dff();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit unsigned integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator byte(decimal)")]
	public extern static Number _a8bfc1feb93c39cb();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to an 8-bit signed integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator sbyte(decimal)")]
	public extern static Number _824c1dbd3e6691ba();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a Unicode character.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator char(decimal)")]
	public extern static Number _e2c93b47df7960a8();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator short(decimal)")]
	public extern static Number _8f4ca64a21fb08cc();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 16-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator ushort(decimal)")]
	public extern static Number _3e209c4283c6e05e();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator int(decimal)")]
	public extern static Number _bc03e302b86b6800();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 32-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator uint(decimal)")]
	public extern static Number _dea1c1c9c8f2b495();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit signed integer.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator long(decimal)")]
	public extern static BigInt _df6860f57d568704();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a 64-bit unsigned integer. This API is not CLS-compliant.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator ulong(decimal)")]
	public extern static BigInt _047386be34a2d276();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a single-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator float(decimal)")]
	public extern static Number _2de5f5a183f9455b();

	///<summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> to a double-precision floating-point number.</summary>
	[Jazor(Op.Discard ,"static decimal.explicit operator double(decimal)")]
	public extern static Number _2db2eb304fe215ee();

	///<summary>Returns the value of the <see cref="T:System.Decimal" /> operand (the sign of the operand is unchanged).</summary>
	[Jazor(Op.Allowed ,"static decimal.operator +(decimal)")]
	public extern static string _53fb6447e19a3943(string d);

	///<summary>Negates the value of the specified <see cref="T:System.Decimal" /> operand.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator -(decimal)")]
	public extern static string _ec128cb5140788f6(string d);

	///<summary>Increments the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by 1.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator ++(decimal)")]
	public extern static string _20e1c565f1757f95(string d);

	///<summary>Decrements the <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> operand by one.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator --(decimal)")]
	public extern static string _92103936e252998e(string d);

	///<summary>Adds two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator +(decimal, decimal)")]
	public extern static string _6916013808c205d4(string d1, string d2);

	///<summary>Subtracts two specified <see cref="T:System.Decimal" /> values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator -(decimal, decimal)")]
	public extern static string _7b8c963ebbb0237b(string d1, string d2);

	///<summary>Multiplies two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator *(decimal, decimal)")]
	public extern static string _5794746a3d1c5c7d(string d1, string d2);

	///<summary>Divides two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator /(decimal, decimal)")]
	public extern static string _18540fea4c4d81f3(string d1, string d2);

	///<summary>Returns the remainder resulting from dividing two specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator %(decimal, decimal)")]
	public extern static string _cf5ffdcf799ce372(string d1, string d2);

	///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> values are equal.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator ==(decimal, decimal)")]
	public extern static bool _9831be72bebc3a57(string d1, string d2);

	///<summary>Returns a value that indicates whether two <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> objects have different values.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator !=(decimal, decimal)")]
	public extern static bool _6e351e0d21e0ccd9(string d1, string d2);

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator <(decimal, decimal)")]
	public extern static bool _9e3b1978bc32f62a(string d1, string d2);

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator <=(decimal, decimal)")]
	public extern static bool _01544ed3b8bf9a49(string d1, string d2);

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator >(decimal, decimal)")]
	public extern static bool _bb8c4bd3620de56b(string d1, string d2);

	///<summary>Returns a value indicating whether a specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.Decimal"></xref>.</summary>
	[Jazor(Op.Allowed ,"static decimal.operator >=(decimal, decimal)")]
	public extern static bool _325daf3875076acb(string d1, string d2);

	///<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Decimal" />.</summary>
	[Jazor(Op.Discard ,"decimal.GetTypeCode()")]
	public extern static System.TypeCode _323e061741a92593(string instance);

	///<summary>Converts a value to a specified integer type using saturation on overflow</summary>
	[Jazor(Op.Discard ,"static decimal.ConvertToInteger<TInteger>(decimal)")]
	public extern static TInteger _3c8005c9c5a1e322<TInteger>(string value);

	///<summary>Converts a value to a specified integer type using platform specific behavior on overflow.</summary>
	[Jazor(Op.Discard ,"static decimal.ConvertToIntegerNative<TInteger>(decimal)")]
	public extern static TInteger _c3fce0dbb13c48ea<TInteger>(string value);

	///<summary>Clamps a value to an inclusive minimum and maximum value.</summary>
	[Jazor(Op.Discard ,"static decimal.Clamp(decimal, decimal, decimal)")]
	public extern static string _e886400fbfdbdaaa(string value, string min, string max);

	///<summary>Copies the sign of a value to the sign of another value.</summary>
	[Jazor(Op.Discard ,"static decimal.CopySign(decimal, decimal)")]
	public extern static string _30df447725c40575(string value, string sign);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static decimal.Max(decimal, decimal)")]
	public extern static string _872018e11335480a(string x, string y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static decimal.Min(decimal, decimal)")]
	public extern static string _ceb21f954af742e7(string x, string y);

	///<summary>Computes the sign of a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Sign(decimal)")]
	public extern static Number _ed803cf9c8c052f1(string d);

	///<summary>Computes the absolute of a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Abs(decimal)")]
	public extern static string _e85678b4de2283e8(string value);

	///<summary>Creates an instance of the current type from a value, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateChecked<TOther>(TOther)")]
	public extern static string _1db5e716e3d6b295<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, saturating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateSaturating<TOther>(TOther)")]
	public extern static string _0263284f14d9d42b<TOther>(object value);

	///<summary>Creates an instance of the current type from a value, truncating any values that fall outside the representable range of the current type.</summary>
	[Jazor(Op.Discard ,"static decimal.CreateTruncating<TOther>(TOther)")]
	public extern static string _5c966a3c7ee1bf4c<TOther>(object value);

	///<summary>Determines if a value is in its canonical representation.</summary>
	[Jazor(Op.Discard ,"static decimal.IsCanonical(decimal)")]
	public extern static bool _b80d517d733633a6(string value);

	///<summary>Determines if a value represents an even integral number.</summary>
	[Jazor(Op.Discard ,"static decimal.IsEvenInteger(decimal)")]
	public extern static bool _9d28fa751d24ce2e(string value);

	///<summary>Determines if a value represents an integral number.</summary>
	[Jazor(Op.Discard ,"static decimal.IsInteger(decimal)")]
	public extern static bool _e79590278b446432(string value);

	///<summary>Determines if a value is negative.</summary>
	[Jazor(Op.Discard ,"static decimal.IsNegative(decimal)")]
	public extern static bool _1ad42f1c78dbe014(string value);

	///<summary>Determines if a value represents an odd integral number.</summary>
	[Jazor(Op.Discard ,"static decimal.IsOddInteger(decimal)")]
	public extern static bool _38587400d9c44cb5(string value);

	///<summary>Determines if a value is positive.</summary>
	[Jazor(Op.Discard ,"static decimal.IsPositive(decimal)")]
	public extern static bool _03c325899b0e33f0(string value);

	///<summary>Compares two values to compute which is greater.</summary>
	[Jazor(Op.Discard ,"static decimal.MaxMagnitude(decimal, decimal)")]
	public extern static string _becce0ac49342bb2(string x, string y);

	///<summary>Compares two values to compute which is lesser.</summary>
	[Jazor(Op.Discard ,"static decimal.MinMagnitude(decimal, decimal)")]
	public extern static string _5df17b0a512de878(string x, string y);

	///<summary>Tries to parse a string into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(string, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _a3ffdb214a9c82a0(string? s, Intl.NumberFormat? provider, string result);

	///<summary>Parses a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
	public extern static string _c644fa2b15360347(string s, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _7ac8df441c1485cf(string s, Intl.NumberFormat? provider, string result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)")]
	public extern static string _e81acb76373d457e(Uint8Array utf8Text, object style, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _acbda6e104ca3de4(Uint8Array utf8Text, object style, Intl.NumberFormat? provider, string result);

	///<summary>Parses a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)")]
	public extern static string _d3d821054d142668(Uint8Array utf8Text, Intl.NumberFormat? provider);

	///<summary>Tries to parse a span of UTF-8 characters into a value.</summary>
	[Jazor(Op.Discard ,"static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)")]
	public extern static Array<object?> _8122c647766e18ff(Uint8Array utf8Text, Intl.NumberFormat? provider, string result);
}
