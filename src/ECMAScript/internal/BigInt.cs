using static ECMAScript.Global;
namespace ECMAScript;

[ECMAScript]
[Description("@#BigInt")]
public abstract class BigInt
{
	[Description("@#0n")]
	public static readonly BigInt Zero = BigInt(0);

	[Description("@#1n")]
	public static readonly BigInt One = BigInt(1);

	[Description("@#2n")]
	public static readonly BigInt Two = BigInt(2);

	[Description("@#-1n")]
	public static readonly BigInt MinusOne = BigInt(-1);

	/// <summary>
	/// 将 BigInt 值转换为一个 -2^(width-1) 与 2^(width-1)-1 之间的有符号整数。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为有符号整数的值。</returns>
	[Description("@#asIntN")]
	public extern static BigInt AsIntN(Number width, Number bigint);

	/// <summary>
	/// 将 BigInt 值转换为一个 -2^(width-1) 与 2^(width-1)-1 之间的无符号整数。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为无符号整数的值。</returns>
	[Description("@#asUintN")]
	public extern static BigInt AsUintN(Number width, Number bigint);

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

	public extern static BigInt operator %(BigInt x, BigInt y);

	public extern override bool Equals(object? obj);

	public extern override int GetHashCode();
}
