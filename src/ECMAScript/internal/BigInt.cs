namespace ECMAScript;

[ECMAScript]
[Description("@#BigInt")]
[Jazor]
public abstract class BigInt
{
	[Jazor("0n")]
	public extern static BigInt Zero { get; }

	[Jazor("1n")]
	public extern static BigInt One { get; }

	[Jazor("2n")]
	public extern static BigInt Two { get; }

	[Jazor("-1n")]
	public extern static BigInt MinusOne { get; }

	/// <summary>
	/// 将 BigInt 值转换为一个 -2^(width-1) 与 2^(width-1)-1 之间的有符号整数。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为有符号整数的值。</returns>
	[Description("@#asIntN")]
	public extern static BigInt AsIntN(Number width, BigInt bigint);

	/// <summary>
	/// 将 BigInt 值转换为一个 -2^(width-1) 与 2^(width-1)-1 之间的无符号整数。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为无符号整数的值。</returns>
	[Description("@#asUintN")]
	public extern static BigInt AsUintN(Number width, BigInt bigint);

	/// <summary>
	/// Returns a string representation of the JavaScript bigint value.
	/// The optional radix stays on the instance surface because JavaScript exposes it as <c>BigInt.prototype.toString</c>.
	/// </summary>
	[Description("@#toString")]
	public extern string ToString(Number? radix);

	/// <summary>
	/// Returns a locale-sensitive string representation of the JavaScript bigint value.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales = null, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// Returns a locale-sensitive string representation of the JavaScript bigint value.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string[] locales, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// Returns the primitive bigint value carried by this host projection.
	/// </summary>
	[Description("@#valueOf")]
	public extern BigInt ValueOf();

	[Description("@#toString")]
	public extern override string ToString();

	public extern static BigInt operator +(BigInt a, BigInt b);

	public extern static BigInt operator -(BigInt a, BigInt b);

	public extern static BigInt operator -(BigInt a);

	public extern static BigInt operator *(BigInt a, BigInt b);

	public extern static BigInt operator /(BigInt a, BigInt b);

	public extern static bool operator ==(BigInt a, BigInt b);

	public extern static bool operator !=(BigInt a, BigInt b);

	public extern static BigInt operator ++(BigInt x);

	public extern static BigInt operator --(BigInt x);

	public extern static bool operator >(BigInt x, BigInt y);

	public extern static bool operator >=(BigInt x, BigInt y);

	public extern static bool operator >=(BigInt x, Number y);

	public extern static bool operator <(BigInt x, BigInt y);

	public extern static bool operator <=(BigInt x, BigInt y);

	public extern static bool operator <=(BigInt x, Number y);

	public extern static BigInt operator >>(BigInt x, BigInt y);

	public extern static BigInt operator <<(BigInt x, BigInt y);

	public extern static BigInt operator |(BigInt x, BigInt y);

	public extern static BigInt operator &(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript bigint bitwise xor.
	/// </summary>
	public extern static BigInt operator ^(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript bigint bitwise not.
	/// </summary>
	public extern static BigInt operator ~(BigInt x);

	public extern static BigInt operator %(BigInt x, BigInt y);

	[ECMAScriptIgnore]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	[ECMAScriptIgnore]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();
}
