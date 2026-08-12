namespace ECMAScript;

[ECMAScript]
[Description("@#Number")]
/// <summary>
/// Strongly typed C# authoring binding for JavaScript <c>Number</c>.
/// JavaScript <c>Number</c> 的强类型 C# 编写绑定。
/// </summary>
/// <remarks>
/// JavaScript numbers are IEEE-754 double-precision values and cannot exactly represent every CLR integer or <c>decimal</c> value.
/// CLR conversion behavior is determined by Jazor.CLR and the compiler; this type describes only the JavaScript Number operation surface.
/// Number 表示 IEEE-754 双精度值，不应误认为能够精确承载所有 CLR 整数或 <c>decimal</c> 值。
/// 具体 CLR 转换行为由 Jazor.CLR 和编译器决定；此类型本身只描述 JavaScript Number 的运算表面。
/// </remarks>
public readonly struct Number : IEquatable<Number>, IComparable, IComparable<Number>, IMinMaxValue<Number>, IFormattable
{
	/// <summary>Creates a JavaScript number from an unsigned byte. 从无符号字节创建 JavaScript Number。</summary>
	public extern Number(byte value);

	/// <summary>Creates a JavaScript number from a signed byte. 从有符号字节创建 JavaScript Number。</summary>
	public extern Number(sbyte value);

	/// <summary>Creates a JavaScript number from a signed 16-bit integer. 从有符号 16 位整数创建 JavaScript Number。</summary>
	public extern Number(short value);

	/// <summary>Creates a JavaScript number from an unsigned 16-bit integer. 从无符号 16 位整数创建 JavaScript Number。</summary>
	public extern Number(ushort value);

	/// <summary>Creates a JavaScript number from a signed 32-bit integer. 从有符号 32 位整数创建 JavaScript Number。</summary>
	public extern Number(int value);

	/// <summary>Creates a JavaScript number from an unsigned 32-bit integer. 从无符号 32 位整数创建 JavaScript Number。</summary>
	public extern Number(uint value);

	//public extern Number(long value);

	//public extern Number(ulong value);

	/// <summary>Creates a JavaScript number from a single-precision value. 从单精度值创建 JavaScript Number。</summary>
	public extern Number(float value);

	/// <summary>Creates a JavaScript number from a double-precision value. 从双精度值创建 JavaScript Number。</summary>
	public extern Number(double value);

	/// <summary>Creates a JavaScript number from a CLR decimal value; precision follows JavaScript Number conversion. 从 CLR decimal 创建 JavaScript Number，精度遵循 JavaScript Number 转换规则。</summary>
	public extern Number(decimal value);

	/// <summary>Implicitly projects an unsigned byte to JavaScript Number. 将无符号字节隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(byte value);

	/// <summary>Implicitly projects a signed byte to JavaScript Number. 将有符号字节隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(sbyte value);

	/// <summary>Implicitly projects a signed 16-bit integer to JavaScript Number. 将有符号 16 位整数隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(short value);

	/// <summary>Implicitly projects an unsigned 16-bit integer to JavaScript Number. 将无符号 16 位整数隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(ushort value);

	/// <summary>Implicitly projects a signed 32-bit integer to JavaScript Number. 将有符号 32 位整数隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(int value);

	/// <summary>Implicitly projects an unsigned 32-bit integer to JavaScript Number. 将无符号 32 位整数隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(uint value);

	//public extern static implicit operator Number(long value);

	//public extern static implicit operator Number(ulong value);

	/// <summary>Implicitly projects a single-precision value to JavaScript Number. 将单精度值隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(float value);

	/// <summary>Implicitly projects a double-precision value to JavaScript Number. 将双精度值隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(double value);

	/// <summary>Implicitly projects a CLR decimal value to JavaScript Number. 将 CLR decimal 隐式投影为 JavaScript Number。</summary>
	public extern static implicit operator Number(decimal value);

	/// <summary>Projects a JavaScript Number to an unsigned byte according to the compiler/runtime conversion contract. 按编译器/运行时转换契约将 JavaScript Number 投影为无符号字节。</summary>
	public extern static implicit operator byte(Number value);

	/// <summary>Projects a JavaScript Number to a signed byte. 将 JavaScript Number 投影为有符号字节。</summary>
	public extern static implicit operator sbyte(Number value);

	/// <summary>Projects a JavaScript Number to a signed 16-bit integer. 将 JavaScript Number 投影为有符号 16 位整数。</summary>
	public extern static implicit operator short(Number value);

	/// <summary>Projects a JavaScript Number to an unsigned 16-bit integer. 将 JavaScript Number 投影为无符号 16 位整数。</summary>
	public extern static implicit operator ushort(Number value);

	/// <summary>Projects a JavaScript Number to a signed 32-bit integer. 将 JavaScript Number 投影为有符号 32 位整数。</summary>
	public extern static implicit operator int(Number value);

	/// <summary>Projects a JavaScript Number to an unsigned 32-bit integer. 将 JavaScript Number 投影为无符号 32 位整数。</summary>
	public extern static implicit operator uint(Number value);

	//public extern static implicit operator long(Number value);

	//public extern static implicit operator ulong(Number value);

	/// <summary>Projects a JavaScript Number to a single-precision value. 将 JavaScript Number 投影为单精度值。</summary>
	public extern static implicit operator float(Number value);

	/// <summary>Projects a JavaScript Number to a double-precision value. 将 JavaScript Number 投影为双精度值。</summary>
	public extern static implicit operator double(Number value);

	/// <summary>Projects a JavaScript Number to a CLR decimal value. 将 JavaScript Number 投影为 CLR decimal。</summary>
	public extern static implicit operator decimal(Number value);

	/// <summary>Adds two JavaScript numbers. 按 JavaScript Number 语义相加两个数值。</summary>
	public extern static Number operator +(Number a, Number b);

	/// <summary>Subtracts two JavaScript numbers. 按 JavaScript Number 语义相减两个数值。</summary>
	public extern static Number operator -(Number a, Number b);

	/// <summary>Multiplies two JavaScript numbers. 按 JavaScript Number 语义相乘两个数值。</summary>
	public extern static Number operator *(Number a, Number b);

	/// <summary>Divides two JavaScript numbers; zero and non-finite cases follow JavaScript semantics. 按 JavaScript Number 语义相除，零与非有限值情形遵循 JavaScript。</summary>
	public extern static Number operator /(Number a, Number b);

	//public extern static bool operator ==(Number a, Number b);

	/// <summary>Uses JavaScript equality lowering for Number and a runtime value. 对 Number 与运行时值使用 JavaScript 相等比较 lowering。</summary>
	public extern static bool operator ==(Number a, object? b);

	//public extern static bool operator ==(object? a, Number b);

	//public extern static bool operator !=(Number a, Number b);

	/// <summary>Uses JavaScript inequality lowering for Number and a runtime value. 对 Number 与运行时值使用 JavaScript 不等比较 lowering。</summary>
	public extern static bool operator !=(Number a, object? b);

	//public extern static bool operator !=(object? a, Number b);

	/// <summary>Increments a JavaScript Number. 对 JavaScript Number 执行递增。</summary>
	public extern static Number operator ++(Number x);

	/// <summary>Decrements a JavaScript Number. 对 JavaScript Number 执行递减。</summary>
	public extern static Number operator --(Number x);

	/// <summary>Compares whether the left JavaScript Number is greater. 比较左侧 JavaScript Number 是否更大。</summary>
	public extern static bool operator >(Number x, Number y);

	/// <summary>Compares whether the left JavaScript Number is greater than or equal. 比较左侧 JavaScript Number 是否大于等于右侧。</summary>
	public extern static bool operator >=(Number x, Number y);

	/// <summary>Compares whether the left JavaScript Number is less. 比较左侧 JavaScript Number 是否更小。</summary>
	public extern static bool operator <(Number x, Number y);

	/// <summary>Compares whether the left JavaScript Number is less than or equal. 比较左侧 JavaScript Number 是否小于等于右侧。</summary>
	public extern static bool operator <=(Number x, Number y);

	/// <summary>
	/// Returns a string representation of an object.
	/// 按指定进制返回 JavaScript Number 的字符串表示；省略进制时使用十进制。
	/// </summary>
	/// <param name="radix">Specifies a radix for converting numeric values to strings.This value is only used for numbers.</param>
	/// <returns></returns>
	[Description("@#toString")]
	public extern string ToString(Number? radix);

	/// <summary>
	/// Returns a string representing a number in fixed-point notation.
	/// 使用 JavaScript <c>toFixed</c> 的固定小数表示；位数范围错误由 JavaScript 运行时处理。
	/// </summary>
	/// <param name="fractionDigits">Number of digits after the decimal point.Must be in the range 0 - 20, inclusive.</param>
	/// <returns></returns>
	[Description("@#toFixed")]
	public extern string ToFixed(Number? fractionDigits = null);

	/// <summary>
	/// Returns a string containing a number represented in exponential notation.
	/// 使用 JavaScript <c>toExponential</c> 的指数表示。
	/// </summary>
	/// <param name="fractionDigits">Number of digits after the decimal point. Must be in the range 0 - 20, inclusive.</param>
	/// <returns></returns>
	[Description("@#toExponential")]
	public extern string ToExponential(Number? fractionDigits = null);

	/// <summary>
	/// Returns a string containing a number represented either in exponential or fixed-point notation with a specified number of digits.
	/// 按指定有效数字返回指数或定点表示，遵循 JavaScript <c>toPrecision</c> 选择规则。
	/// </summary>
	/// <param name="precision">Number of significant digits.Must be in the range 1 - 21, inclusive.</param>
	/// <returns></returns>
	[Description("@#toPrecision")]
	public extern string ToPrecision(Number? precision = null);

	/// <summary>
	/// Returns the primitive value of the specified object.
	/// 返回 JavaScript Number 原始值；该方法主要用于与原生 <c>valueOf()</c> 形状对齐。
	/// </summary>
	/// <returns></returns>
	[Description("@#valueOf")]
	public extern Number ValueOf();

	/// <summary>Gets the JavaScript <c>Number.prototype</c> object. 获取 JavaScript <c>Number.prototype</c> 对象。</summary>
	[Description("@#prototype")]
	public extern static Number Prototype { get; }

	/// <summary>
	/// Smallest interval between 1 and the next representable JavaScript number.
	/// This is the static <c>Number.EPSILON</c> host member, not a CLR numeric helper.
	/// JavaScript 中 1 与下一个可表示数之间的最小间隔；这是静态 <c>Number.EPSILON</c> 宿主成员，不是 CLR 数值帮助器。
	/// </summary>
	[Description("@#EPSILON")]
	public extern static Number EPSILON { get; }

	/// <summary>
	/// The largest number that can be represented in JavaScript. Equal to approximately 1.79E+308.
	/// JavaScript 可表示的最大有限 Number，约为 <c>1.79E+308</c>。
	/// </summary>
	[Description("@#MAX_VALUE")]
	public extern static Number MAX_VALUE { get; }

	/// <summary>
	/// The closest number to zero that can be represented in JavaScript. Equal to approximately 5.00E-324.
	/// 最接近零的正 JavaScript Number，约为 <c>5.00E-324</c>，不是最小负数。
	/// </summary>
	[Description("@#MIN_VALUE")]
	public extern static Number MIN_VALUE { get; }

	/// <summary>
	/// A value that is not a number.
	/// In equality comparisons, NaN does not equal any value, including itself.To test whether a value is equivalent to NaN, use the isNaN function.
	/// 非数值常量。相等比较中 <c>NaN</c> 不等于任何值（包括自身）；应使用 <see cref="IsNaN"/> 检查。
	/// </summary>
	[Description("@#NaN")]
	public extern static Number NaN { get; }

	/// <summary>
	/// A value that is less than the largest negative number that can be represented in JavaScript.
	/// JavaScript displays NEGATIVE_INFINITY values as -infinity.
	/// 小于任何有限值的负无穷；JavaScript 通常显示为 <c>-Infinity</c>。
	/// </summary>
	[Description("@#NEGATIVE_INFINITY")]
	public extern static Number NEGATIVE_INFINITY { get; }

	/// <summary>
	/// A value greater than the largest number that can be represented in JavaScript.
	/// JavaScript displays POSITIVE_INFINITY values as infinity.
	/// 大于任何有限值的正无穷；JavaScript 通常显示为 <c>Infinity</c>。
	/// </summary>
	[Description("@#POSITIVE_INFINITY")]
	public extern static Number POSITIVE_INFINITY { get; }

	/// <summary>Gets the smallest integer exactly representable by JavaScript Number. 获取 JavaScript Number 可精确表示的最小整数。</summary>
	[Description("@#MIN_SAFE_INTEGER")]
	public extern static Number MIN_SAFE_INTEGER { get; }

	/// <summary>Gets the largest integer exactly representable by JavaScript Number. 获取 JavaScript Number 可精确表示的最大整数。</summary>
	[Description("@#MAX_SAFE_INTEGER")]
	public extern static Number MAX_SAFE_INTEGER { get; }

	/// <summary>
	/// Converts a number to a string by using the current or specified locale.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// 使用 JavaScript 运行时的当前或指定 locale 格式化数值；<see cref="IEnumerable{T}"/> 用于表达 locale 列表。
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// C# 便利重载，用于只传递格式化选项而省略前置 locale 参数。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(Intl.NumberFormatOptions options);

	/// <summary>
	/// Converts a number to a string by using the current or specified locale.
	/// 使用 JavaScript 当前或指定 locale 格式化数值。
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, Intl.NumberFormatOptions? options = null);

	/// <summary>Returns JavaScript <c>Number.prototype.toString()</c> with its default radix. 使用默认进制返回 JavaScript <c>Number.prototype.toString()</c> 结果。</summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>Checks whether a value is a finite integral JavaScript Number. 检查值是否为有限且无小数部分的 JavaScript Number。</summary>
	[Description("@#isInteger")]
	public extern static bool IsInteger(object? value);

	/// <summary>
	/// JavaScript <c>Number.parseFloat</c> alias.
	/// This stays on the <c>Number</c> constructor host because JavaScript exposes it there in addition to the global function.
	/// JavaScript <c>Number.parseFloat</c> 别名；除全局函数外，JavaScript 也在 <c>Number</c> 构造器宿主上暴露它。
	/// </summary>
	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value);

	/// <summary>
	/// JavaScript <c>Number.parseInt</c> alias.
	/// The optional radix matches the standard constructor-host signature.
	/// JavaScript <c>Number.parseInt</c> 别名；可选 radix 与标准构造器宿主签名一致。
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, Number? radix = null);

	/// <summary>
	/// JavaScript <c>Number.isFinite</c> check.
	/// Unlike global <c>isFinite</c>, this static host does not apply JavaScript number coercion first.
	/// JavaScript <c>Number.isFinite</c> 检查；不同于全局 <c>isFinite</c>，不会先进行 Number 强制转换。
	/// </summary>
	[Description("@#isFinite")]
	public extern static bool IsFinite(object? value);

	/// <summary>
	/// JavaScript <c>Number.isNaN</c> check.
	/// Unlike global <c>isNaN</c>, this static host only returns <see langword="true"/> for actual numeric <c>NaN</c> values.
	/// JavaScript <c>Number.isNaN</c> 检查；不同于全局 <c>isNaN</c>，只对实际数值 <c>NaN</c> 返回 <see langword="true"/>。
	/// </summary>
	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);

	/// <summary>
	/// Returns whether the supplied value is a safe JavaScript integer.
	/// This belongs on the <c>Number</c> constructor host because JavaScript exposes it as <c>Number.isSafeInteger</c>.
	/// 检查值是否为安全 JavaScript 整数；该成员属于 <c>Number</c> 构造器宿主，因为 JavaScript 将其暴露为 <c>Number.isSafeInteger</c>。
	/// </summary>
	[Description("@#isSafeInteger")]
	public extern static bool IsSafeInteger(object? value);

	/// <summary>Internal compiler/runtime marker for a NaN value. 供编译器/运行时识别 NaN 值的内部标记。</summary>
	internal bool IsNaNValue { get; }

	/// <summary>Internal carrier value used by the host projection. 宿主投影使用的内部承载值。</summary>
	internal double Value { get; }

	/// <summary>CLR object-equality bridge; JavaScript equality operators remain the normal authoring surface. CLR 对象相等性桥接；正常编写应使用 JavaScript 相等运算符表面。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	/// <summary>CLR hash-code bridge; it does not introduce a JavaScript <c>Number</c> member. CLR 哈希码桥接，不会引入 JavaScript <c>Number</c> 成员。</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	extern int IComparable.CompareTo(object? obj);

	extern int IComparable<Number>.CompareTo(Number other);

	extern bool IEquatable<Number>.Equals(Number other);

	extern string IFormattable.ToString(string? format, IFormatProvider? formatProvider);

	extern static Number IMinMaxValue<Number>.MaxValue { get; }

	extern static Number IMinMaxValue<Number>.MinValue { get; }
}
