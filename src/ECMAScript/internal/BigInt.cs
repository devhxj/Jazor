namespace ECMAScript;

[ECMAScript]
[Description("@#BigInt")]
[Jazor]
/// <summary>
/// Integer-precision host binding for JavaScript <c>BigInt</c>.
/// JavaScript <c>BigInt</c> 的整数精度宿主绑定。
/// </summary>
/// <remarks>
/// BigInt cannot participate in ordinary arithmetic with Number; callers and CLR modules must convert explicitly at the boundary.
/// This type expresses the JavaScript integer runtime, not a complete substitute for arbitrary-precision <c>decimal</c> or CLR <c>BigInteger</c>.
/// BigInt 不能与 Number 混合参与普通算术；调用方和 CLR module 必须在边界显式转换。
/// 该类型用于表达 JavaScript 整数运行时，不是任意精度 decimal 或 CLR BigInteger 的完整替代品。
/// </remarks>
public abstract class BigInt
{
	/// <summary>
	/// JavaScript <c>BigInt.prototype</c> object.
	/// This stays on the constructor host to keep the public surface aligned with the JavaScript runtime shape.
	/// JavaScript <c>BigInt.prototype</c> 对象；保留在构造器宿主上以保持公开表面与 JavaScript 运行时形状一致。
	/// </summary>
	[Description("@#prototype")]
	public extern static BigInt Prototype { get; }

	/// <summary>Gets the JavaScript bigint literal <c>0n</c>. 获取 JavaScript bigint 字面量 <c>0n</c>。</summary>
	[Jazor("0n")]
	public extern static BigInt Zero { get; }

	/// <summary>Gets the JavaScript bigint literal <c>1n</c>. 获取 JavaScript bigint 字面量 <c>1n</c>。</summary>
	[Jazor("1n")]
	public extern static BigInt One { get; }

	/// <summary>Gets the JavaScript bigint literal <c>2n</c>. 获取 JavaScript bigint 字面量 <c>2n</c>。</summary>
	[Jazor("2n")]
	public extern static BigInt Two { get; }

	/// <summary>Gets the JavaScript bigint literal <c>-1n</c>. 获取 JavaScript bigint 字面量 <c>-1n</c>。</summary>
	[Jazor("-1n")]
	public extern static BigInt MinusOne { get; }

	/// <summary>
	/// Converts a bigint to its signed two's-complement value within the supplied bit width.
	/// 将 BigInt 值转换为宽度为 <paramref name="width"/> 的有符号二进制补码值，范围为 -2^(width-1) 至 2^(width-1)-1。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为有符号整数的值。</returns>
	[Description("@#asIntN")]
	public extern static BigInt AsIntN(Number width, BigInt bigint);

	/// <summary>
	/// Converts a bigint to its unsigned value modulo 2^width.
	/// 将 BigInt 值转换为宽度为 <paramref name="width"/> 的无符号值，即对 2^width 取模。
	/// </summary>
	/// <param name="width">可存储整数的位数。</param>
	/// <param name="bigint">要存储在指定位数上的整数。</param>
	/// <returns>bigint 模 (modulo) 2^width 作为无符号整数的值。</returns>
	[Description("@#asUintN")]
	public extern static BigInt AsUintN(Number width, BigInt bigint);

	/// <summary>
	/// Returns a string representation of the JavaScript bigint value.
	/// The optional radix stays on the instance surface because JavaScript exposes it as <c>BigInt.prototype.toString</c>.
	/// 返回 JavaScript bigint 的文本表示；可选 radix 保留在实例表面，因为 JavaScript 将其定义为 <c>BigInt.prototype.toString</c>。
	/// </summary>
	[Description("@#toString")]
	public extern string ToString(Number? radix);

	/// <summary>
	/// Returns a locale-sensitive string representation of the JavaScript bigint value.
	/// 使用 JavaScript 当前或指定 locale 格式化 bigint 值。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales = null, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// C# 便利重载，用于仅传格式化选项并省略前置 locale 参数。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(Intl.NumberFormatOptions options);

	/// <summary>
	/// Returns a locale-sensitive string representation of the JavaScript bigint value.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// 使用 JavaScript locale 列表格式化 bigint；<see cref="IEnumerable{T}"/> 是 locale 列表的通用 C# 输入表面。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string> locales, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// Returns the primitive bigint value carried by this host projection.
	/// 返回此宿主投影承载的 JavaScript bigint 原始值。
	/// </summary>
	[Description("@#valueOf")]
	public extern BigInt ValueOf();

	/// <summary>Returns the default-radix JavaScript bigint string. 使用默认进制返回 JavaScript bigint 字符串。</summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>Adds two JavaScript bigint values. 相加两个 JavaScript bigint 值。</summary>
	public extern static BigInt operator +(BigInt a, BigInt b);

	/// <summary>Subtracts two JavaScript bigint values. 相减两个 JavaScript bigint 值。</summary>
	public extern static BigInt operator -(BigInt a, BigInt b);

	/// <summary>Negates a JavaScript bigint value. 对 JavaScript bigint 值取负。</summary>
	public extern static BigInt operator -(BigInt a);

	/// <summary>Multiplies two JavaScript bigint values. 相乘两个 JavaScript bigint 值。</summary>
	public extern static BigInt operator *(BigInt a, BigInt b);

	/// <summary>Divides bigint values with JavaScript truncation-toward-zero semantics. 按 JavaScript 向零截断语义相除 bigint 值。</summary>
	public extern static BigInt operator /(BigInt a, BigInt b);

	/// <summary>Compares two bigint values for JavaScript equality. 比较两个 bigint 值是否按 JavaScript 相等。</summary>
	public extern static bool operator ==(BigInt a, BigInt b);

	/// <summary>Compares two bigint values for JavaScript inequality. 比较两个 bigint 值是否按 JavaScript 不等。</summary>
	public extern static bool operator !=(BigInt a, BigInt b);

	/// <summary>Increments a JavaScript bigint value. 对 JavaScript bigint 值执行递增。</summary>
	public extern static BigInt operator ++(BigInt x);

	/// <summary>Decrements a JavaScript bigint value. 对 JavaScript bigint 值执行递减。</summary>
	public extern static BigInt operator --(BigInt x);

	/// <summary>Compares two bigint values by JavaScript relational ordering. 按 JavaScript 关系顺序比较两个 bigint 值。</summary>
	public extern static bool operator >(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript allows relational comparison between bigint and number values.
	/// These mixed overloads keep that runtime surface available in C# without implying that arithmetic mixing is valid.
	/// JavaScript 允许 bigint 与 Number 进行关系比较；这些重载保留该运行时表面，但不表示二者可以混合算术。
	/// </summary>
	public extern static bool operator >(BigInt x, Number y);

	/// <summary>Compares whether one bigint is greater than or equal to another. 比较一个 bigint 是否大于等于另一个 bigint。</summary>
	public extern static bool operator >=(BigInt x, BigInt y);

	/// <summary>Compares a bigint and Number without enabling mixed arithmetic. 比较 bigint 与 Number，但不启用混合算术。</summary>
	public extern static bool operator >=(BigInt x, Number y);

	/// <summary>Compares two bigint values by JavaScript relational ordering. 按 JavaScript 关系顺序比较两个 bigint 值。</summary>
	public extern static bool operator <(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript allows relational comparison between bigint and number values.
	/// These mixed overloads keep that runtime surface available in C# without implying that arithmetic mixing is valid.
	/// JavaScript 允许 bigint 与 Number 进行关系比较；这些重载保留该运行时表面，但不表示二者可以混合算术。
	/// </summary>
	public extern static bool operator <(BigInt x, Number y);

	/// <summary>Compares whether one bigint is less than or equal to another. 比较一个 bigint 是否小于等于另一个 bigint。</summary>
	public extern static bool operator <=(BigInt x, BigInt y);

	/// <summary>Compares a bigint and Number without enabling mixed arithmetic. 比较 bigint 与 Number，但不启用混合算术。</summary>
	public extern static bool operator <=(BigInt x, Number y);

	/// <summary>
	/// Symmetric mixed relational overloads are exposed so C# can express the same JavaScript comparison surface regardless of operand order.
	/// 提供对称的混合关系比较重载，使 C# 无论操作数顺序都可表达相同 JavaScript 比较表面。
	/// </summary>
	public extern static bool operator >(Number x, BigInt y);

	public extern static bool operator >=(Number x, BigInt y);

	public extern static bool operator <(Number x, BigInt y);

	public extern static bool operator <=(Number x, BigInt y);

	/// <summary>Arithmetic right-shifts a JavaScript bigint. 对 JavaScript bigint 执行算术右移。</summary>
	public extern static BigInt operator >>(BigInt x, BigInt y);

	/// <summary>Left-shifts a JavaScript bigint. 对 JavaScript bigint 执行左移。</summary>
	public extern static BigInt operator <<(BigInt x, BigInt y);

	/// <summary>Performs JavaScript bigint bitwise OR. 对 JavaScript bigint 执行按位 OR。</summary>
	public extern static BigInt operator |(BigInt x, BigInt y);

	/// <summary>Performs JavaScript bigint bitwise AND. 对 JavaScript bigint 执行按位 AND。</summary>
	public extern static BigInt operator &(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript bigint bitwise xor.
	/// JavaScript bigint 按位 XOR。
	/// </summary>
	public extern static BigInt operator ^(BigInt x, BigInt y);

	/// <summary>
	/// JavaScript bigint bitwise not.
	/// JavaScript bigint 按位 NOT。
	/// </summary>
	public extern static BigInt operator ~(BigInt x);

	/// <summary>Returns the JavaScript bigint remainder. 返回 JavaScript bigint 余数。</summary>
	public extern static BigInt operator %(BigInt x, BigInt y);

	/// <summary>CLR equality bridge excluded from ECMAScript lowering. 被 ECMAScript lowering 排除的 CLR 相等性桥接。</summary>
	[ECMAScriptIgnore]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	/// <summary>CLR hash-code bridge excluded from ECMAScript lowering. 被 ECMAScript lowering 排除的 CLR 哈希码桥接。</summary>
	[ECMAScriptIgnore]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();
}
