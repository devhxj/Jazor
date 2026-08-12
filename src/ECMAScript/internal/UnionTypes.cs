using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// Strongly typed union representation of an ECMAScript property key.
/// ECMAScript PropertyKey 的强类型联合表示。
/// </summary>
/// <remarks>
/// <c>PropertyKey</c> permits <see cref="string"/>, <see cref="Number"/>, and <see cref="Symbol"/> authoring branches and erases to the corresponding JavaScript value.
/// The <c>AsX</c> properties retain exact branch projections and must not be treated as a reflectable CLR union object.
/// <c>PropertyKey</c> 允许 <see cref="string"/>、<see cref="Number"/> 和 <see cref="Symbol"/> 三种 authoring 分支，并在编译后擦除为对应 JavaScript 值；
/// <c>AsX</c> 属性保持精确分支投影，不应将其视为可反射的 CLR union 对象。
/// </remarks>
public readonly union PropertyKey(string, Number, Symbol)
{
	/// <summary>Gets the string branch, or <see langword="null"/> for another branch. 获取 string 分支；其他分支时为 <see langword="null"/>。</summary>
	public string? AsString => Value as string;

	/// <summary>Gets the number branch, or <see langword="null"/> for another branch. 获取 Number 分支；其他分支时为 <see langword="null"/>。</summary>
	public Number? AsNumber => Value is Number value ? value : default(Number?);

	/// <summary>Gets the Symbol branch, or <see langword="null"/> for another branch. 获取 Symbol 分支；其他分支时为 <see langword="null"/>。</summary>
	public Symbol? AsSymbol => Value as Symbol;
}

[ECMAScript]
[Description("@#")]
/// <summary>JavaScript Date constructor primitive input union. JavaScript Date 构造器原始值输入联合。</summary>
public readonly union DatePrimitive(string, Number)
{
	/// <summary>Gets the date-string branch. 获取日期字符串分支。</summary>
	public string? AsString => Value as string;

	/// <summary>Gets the millisecond timestamp branch. 获取毫秒时间戳分支。</summary>
	public Number? AsNumber => Value is Number value ? value : default(Number?);
}

[ECMAScript]
[Description("@#")]
/// <summary>Result value union returned by <c>Atomics.waitAsync</c>. <c>Atomics.waitAsync</c> 返回的结果值联合。</summary>
public readonly union AtomicsWaitAsyncValue(string, Promise<string>)
{
	/// <summary>Gets the synchronous status branch. 获取同步状态分支。</summary>
	public string? AsString => Value as string;

	/// <summary>Gets the asynchronous status Promise branch. 获取异步状态 Promise 分支。</summary>
	public Promise<string>? AsPromise => Value as Promise<string>;
}

[ECMAScript]
[Description("@#")]
/// <summary>JavaScript <c>Intl.NumberFormat.useGrouping</c> input union. JavaScript <c>Intl.NumberFormat.useGrouping</c> 输入联合。</summary>
public readonly union IntlUseGrouping(bool, Intl.NumberFormatUseGrouping)
{
	/// <summary>Gets the Boolean branch. 获取布尔值分支。</summary>
	public bool? AsBool => Value is bool value ? value : default(bool?);

	/// <summary>Gets the string-mode enum branch. 获取字符串模式枚举分支。</summary>
	public Intl.NumberFormatUseGrouping? AsMode => Value is Intl.NumberFormatUseGrouping value ? value : default(Intl.NumberFormatUseGrouping?);
}

[ECMAScript]
[Description("@#")]
/// <summary>Mathematical-value union accepted by ECMA-402 number APIs. ECMA-402 数值 API 接受的数学值联合。</summary>
public readonly union IntlNumberInput(Number, BigInt, string)
{
	/// <summary>Gets the JavaScript number branch. 获取 JavaScript Number 分支。</summary>
	public Number? AsNumber => Value is Number value ? value : default(Number?);

	/// <summary>Gets the JavaScript bigint branch. 获取 JavaScript BigInt 分支。</summary>
	public BigInt? AsBigInt => Value as BigInt;

	/// <summary>Gets the decimal-string branch. 获取十进制字符串分支。</summary>
	public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
/// <summary>Date-or-timestamp union accepted by ECMA-402 date/time APIs. ECMA-402 日期时间 API 接受的 Date 或时间戳联合。</summary>
public readonly union IntlDateTimeInput(Date, Number)
{
	/// <summary>Gets the Date object branch. 获取 Date 对象分支。</summary>
	public Date? AsDate => Value as Date;

	/// <summary>Gets the millisecond timestamp branch. 获取毫秒时间戳分支。</summary>
	public Number? AsNumber => Value is Number value ? value : default(Number?);
}

[ECMAScript]
[Description("@#")]
/// <summary>Month-style union for ECMA-402 date/time formatting. ECMA-402 日期时间格式化的月份样式联合。</summary>
public readonly union IntlMonthStyle(Intl.NumericTwoDigit, Intl.LongShortNarrow)
{
	/// <summary>Gets the numeric or two-digit branch. 获取 numeric 或 two-digit 分支。</summary>
	public Intl.NumericTwoDigit? AsNumeric => Value is Intl.NumericTwoDigit value ? value : default(Intl.NumericTwoDigit?);

	/// <summary>Gets the long, short, or narrow text branch. 获取 long、short 或 narrow 文本分支。</summary>
	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);
}

[ECMAScript]
[Description("@#")]
/// <summary>Duration text-or-numeric style union. Duration 的文本或数值样式联合。</summary>
public readonly union IntlDurationTextStyle(Intl.LongShortNarrow, Intl.NumericTwoDigit)
{
	/// <summary>Gets the textual-style branch. 获取文本样式分支。</summary>
	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);

	/// <summary>Gets the numeric-style branch. 获取数值样式分支。</summary>
	public Intl.NumericTwoDigit? AsNumeric => Value is Intl.NumericTwoDigit value ? value : default(Intl.NumericTwoDigit?);
}

[ECMAScript]
[Description("@#")]
/// <summary>Duration fractional-second text-or-numeric style union. Duration 小数秒的文本或数值样式联合。</summary>
public readonly union IntlDurationFractionStyle(Intl.LongShortNarrow, Intl.DurationNumericStyle)
{
	/// <summary>Gets the textual-style branch. 获取文本样式分支。</summary>
	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);

	/// <summary>Gets the numeric-style branch. 获取数值样式分支。</summary>
	public Intl.DurationNumericStyle? AsNumeric => Value is Intl.DurationNumericStyle value ? value : default(Intl.DurationNumericStyle?);
}
