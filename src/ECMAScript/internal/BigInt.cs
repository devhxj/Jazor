namespace ECMAScript;

[ECMAScript]
[Description("@#BigInt")]
[Jazor]
/// <summary>
/// JavaScript BigInt 的整数精度 host binding。
/// </summary>
/// <remarks>
/// BigInt 不能与 Number 混合参与普通算术；调用方和 CLR module 必须在边界显式转换。
/// 该类型用于表达 JavaScript 整数运行时，不是任意精度 decimal 或 CLR BigInteger 的完整替代品。
/// </remarks>
public abstract class BigInt
{
	/// <summary>
	/// JavaScript <c>BigInt.prototype</c> object.
	/// This stays on the constructor host to keep the public surface aligned with the JavaScript runtime shape.
	/// </summary>
	[Description("@#prototype")]
	public extern static BigInt Prototype { get; }

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
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(Intl.NumberFormatOptions options);

	/// <summary>
	/// Returns a locale-sensitive string representation of the JavaScript bigint value.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string> locales, Intl.NumberFormatOptions? options = null);

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

	/// <summary>
	/// JavaScript allows relational comparison between bigint and number values.
	/// These mixed overloads keep that runtime surface available in C# without implying that arithmetic mixing is valid.
	/// </summary>
	public extern static bool operator >(BigInt x, Number y);

	public extern static bool operator >=(BigInt x, BigInt y);

	public extern static bool operator >=(BigInt x, Number y);

	public extern static bool operator <(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript allows relational comparison between bigint and number values.
	/// These mixed overloads keep that runtime surface available in C# without implying that arithmetic mixing is valid.
	/// </summary>
	public extern static bool operator <(BigInt x, Number y);

	public extern static bool operator <=(BigInt x, BigInt y);

	public extern static bool operator <=(BigInt x, Number y);

	/// <summary>
	/// Symmetric mixed relational overloads are exposed so C# can express the same JavaScript comparison surface regardless of operand order.
	/// </summary>
	public extern static bool operator >(Number x, BigInt y);

	public extern static bool operator >=(Number x, BigInt y);

	public extern static bool operator <(Number x, BigInt y);

	public extern static bool operator <=(Number x, BigInt y);

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
