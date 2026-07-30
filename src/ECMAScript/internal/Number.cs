namespace ECMAScript;

[ECMAScript]
[Description("@#Number")]
/// <summary>
/// JavaScript Number 的强类型 C# authoring binding。
/// </summary>
/// <remarks>
/// Number 表示 IEEE-754 double，不应误认为能够精确承载所有 CLR 整数或 decimal 值。
/// 具体 CLR 类型映射由 Jazor.CLR 和 compiler 决定；此类型本身只描述 JS Number 运算表面。
/// </remarks>
public readonly struct Number : IEquatable<Number>, IComparable, IComparable<Number>, IMinMaxValue<Number>, IFormattable
{
	public extern Number(byte value);

	public extern Number(sbyte value);

	public extern Number(short value);

	public extern Number(ushort value);

	public extern Number(int value);

	public extern Number(uint value);

	//public extern Number(long value);

	//public extern Number(ulong value);

	public extern Number(float value);

	public extern Number(double value);

	public extern Number(decimal value);

	public extern static implicit operator Number(byte value);

	public extern static implicit operator Number(sbyte value);

	public extern static implicit operator Number(short value);

	public extern static implicit operator Number(ushort value);

	public extern static implicit operator Number(int value);

	public extern static implicit operator Number(uint value);

	//public extern static implicit operator Number(long value);

	//public extern static implicit operator Number(ulong value);

	public extern static implicit operator Number(float value);

	public extern static implicit operator Number(double value);

	public extern static implicit operator Number(decimal value);

	public extern static implicit operator byte(Number value);

	public extern static implicit operator sbyte(Number value);

	public extern static implicit operator short(Number value);

	public extern static implicit operator ushort(Number value);

	public extern static implicit operator int(Number value);

	public extern static implicit operator uint(Number value);

	//public extern static implicit operator long(Number value);

	//public extern static implicit operator ulong(Number value);

	public extern static implicit operator float(Number value);

	public extern static implicit operator double(Number value);

	public extern static implicit operator decimal(Number value);

	public extern static Number operator +(Number a, Number b);

	public extern static Number operator -(Number a, Number b);

	public extern static Number operator *(Number a, Number b);

	public extern static Number operator /(Number a, Number b);

	//public extern static bool operator ==(Number a, Number b);

	public extern static bool operator ==(Number a, object? b);

	//public extern static bool operator ==(object? a, Number b);

	//public extern static bool operator !=(Number a, Number b);

	public extern static bool operator !=(Number a, object? b);

	//public extern static bool operator !=(object? a, Number b);

	public extern static Number operator ++(Number x);

	public extern static Number operator --(Number x);

	public extern static bool operator >(Number x, Number y);

	public extern static bool operator >=(Number x, Number y);

	public extern static bool operator <(Number x, Number y);

	public extern static bool operator <=(Number x, Number y);

	/// <summary>
	/// Returns a string representation of an object.
	/// </summary>
	/// <param name="radix">Specifies a radix for converting numeric values to strings.This value is only used for numbers.</param>
	/// <returns></returns>
	[Description("@#toString")]
	public extern string ToString(Number? radix);

	/// <summary>
	/// Returns a string representing a number in fixed-point notation.
	/// </summary>
	/// <param name="fractionDigits">Number of digits after the decimal point.Must be in the range 0 - 20, inclusive.</param>
	/// <returns></returns>
	[Description("@#toFixed")]
	public extern string ToFixed(Number? fractionDigits = null);

	/// <summary>
	/// Returns a string containing a number represented in exponential notation.
	/// </summary>
	/// <param name="fractionDigits">Number of digits after the decimal point. Must be in the range 0 - 20, inclusive.</param>
	/// <returns></returns>
	[Description("@#toExponential")]
	public extern string ToExponential(Number? fractionDigits = null);

	/// <summary>
	/// Returns a string containing a number represented either in exponential or fixed-point notation with a specified number of digits.
	/// </summary>
	/// <param name="precision">Number of significant digits.Must be in the range 1 - 21, inclusive.</param>
	/// <returns></returns>
	[Description("@#toPrecision")]
	public extern string ToPrecision(Number? precision = null);

	/// <summary>
	/// Returns the primitive value of the specified object.
	/// </summary>
	/// <returns></returns>
	[Description("@#valueOf")]
	public extern Number ValueOf();

	[Description("@#prototype")]
	public extern static Number Prototype { get; }

	/// <summary>
	/// Smallest interval between 1 and the next representable JavaScript number.
	/// This is the static <c>Number.EPSILON</c> host member, not a CLR numeric helper.
	/// </summary>
	[Description("@#EPSILON")]
	public extern static Number EPSILON { get; }

	/// <summary>
	/// The largest number that can be represented in JavaScript. Equal to approximately 1.79E+308.
	/// </summary>
	[Description("@#MAX_VALUE")]
	public extern static Number MAX_VALUE { get; }

	/// <summary>
	/// The closest number to zero that can be represented in JavaScript. Equal to approximately 5.00E-324.
	/// </summary>
	[Description("@#MIN_VALUE")]
	public extern static Number MIN_VALUE { get; }

	/// <summary>
	/// A value that is not a number.
	/// In equality comparisons, NaN does not equal any value, including itself.To test whether a value is equivalent to NaN, use the isNaN function.
	/// </summary>
	[Description("@#NaN")]
	public extern static Number NaN { get; }

	/// <summary>
	/// A value that is less than the largest negative number that can be represented in JavaScript.
	/// JavaScript displays NEGATIVE_INFINITY values as -infinity.
	/// </summary>
	[Description("@#NEGATIVE_INFINITY")]
	public extern static Number NEGATIVE_INFINITY { get; }

	/// <summary>
	/// A value greater than the largest number that can be represented in JavaScript.
	/// JavaScript displays POSITIVE_INFINITY values as infinity.
	/// </summary>
	[Description("@#POSITIVE_INFINITY")]
	public extern static Number POSITIVE_INFINITY { get; }

	[Description("@#MIN_SAFE_INTEGER")]
	public extern static Number MIN_SAFE_INTEGER { get; }

	[Description("@#MAX_SAFE_INTEGER")]
	public extern static Number MAX_SAFE_INTEGER { get; }

	/// <summary>
	/// Converts a number to a string by using the current or specified locale.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales, Intl.NumberFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(Intl.NumberFormatOptions options);

	/// <summary>
	/// Converts a number to a string by using the current or specified locale.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, Intl.NumberFormatOptions? options = null);

	[Description("@#toString")]
	public extern override string ToString();

	[Description("@#isInteger")]
	public extern static bool IsInteger(object? value);

	/// <summary>
	/// JavaScript <c>Number.parseFloat</c> alias.
	/// This stays on the <c>Number</c> constructor host because JavaScript exposes it there in addition to the global function.
	/// </summary>
	[Description("@#parseFloat")]
	public extern static Number ParseFloat(object? value);

	/// <summary>
	/// JavaScript <c>Number.parseInt</c> alias.
	/// The optional radix matches the standard constructor-host signature.
	/// </summary>
	[Description("@#parseInt")]
	public extern static Number ParseInt(object? value, Number? radix = null);

	/// <summary>
	/// JavaScript <c>Number.isFinite</c> check.
	/// Unlike global <c>isFinite</c>, this static host does not apply JavaScript number coercion first.
	/// </summary>
	[Description("@#isFinite")]
	public extern static bool IsFinite(object? value);

	/// <summary>
	/// JavaScript <c>Number.isNaN</c> check.
	/// Unlike global <c>isNaN</c>, this static host only returns <see langword="true"/> for actual numeric <c>NaN</c> values.
	/// </summary>
	[Description("@#isNaN")]
	public extern static bool IsNaN(object? value);

	/// <summary>
	/// Returns whether the supplied value is a safe JavaScript integer.
	/// This belongs on the <c>Number</c> constructor host because JavaScript exposes it as <c>Number.isSafeInteger</c>.
	/// </summary>
	[Description("@#isSafeInteger")]
	public extern static bool IsSafeInteger(object? value);

	internal bool IsNaNValue { get; }

	internal double Value { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	extern int IComparable.CompareTo(object? obj);

	extern int IComparable<Number>.CompareTo(Number other);

	extern bool IEquatable<Number>.Equals(Number other);

	extern string IFormattable.ToString(string? format, IFormatProvider? formatProvider);

	extern static Number IMinMaxValue<Number>.MaxValue { get; }

	extern static Number IMinMaxValue<Number>.MinValue { get; }
}
