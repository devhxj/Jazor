using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// ECMAScript PropertyKey 的强类型联合表示。
/// </summary>
/// <remarks>
/// PropertyKey 允许 string、Number 和 Symbol 三种 authoring 分支，并在编译后擦除为对应 JavaScript 值。
/// AsX 属性用于保持分支投影的明确类型，不应把它当作可反射的 CLR union 对象。
/// </remarks>
public readonly union PropertyKey(string, Number, Symbol)
{
	public string? AsString => Value as string;

	public Number? AsNumber => Value is Number value ? value : default(Number?);

	public Symbol? AsSymbol => Value as Symbol;
}

[ECMAScript]
[Description("@#")]
public readonly union DatePrimitive(string, Number)
{
	public string? AsString => Value as string;

	public Number? AsNumber => Value is Number value ? value : default(Number?);
}

[ECMAScript]
[Description("@#")]
public readonly union AtomicsWaitAsyncValue(string, Promise<string>)
{
	public string? AsString => Value as string;

	public Promise<string>? AsPromise => Value as Promise<string>;
}

[ECMAScript]
[Description("@#")]
public readonly union IntlUseGrouping(bool, Intl.NumberFormatUseGrouping)
{
	public bool? AsBool => Value is bool value ? value : default(bool?);

	public Intl.NumberFormatUseGrouping? AsMode => Value is Intl.NumberFormatUseGrouping value ? value : default(Intl.NumberFormatUseGrouping?);
}

[ECMAScript]
[Description("@#")]
public readonly union IntlNumberInput(Number, BigInt, string)
{
	public Number? AsNumber => Value is Number value ? value : default(Number?);

	public BigInt? AsBigInt => Value as BigInt;

	public string? AsString => Value as string;
}

[ECMAScript]
[Description("@#")]
public readonly union IntlDateTimeInput(Date, Number)
{
	public Date? AsDate => Value as Date;

	public Number? AsNumber => Value is Number value ? value : default(Number?);
}

[ECMAScript]
[Description("@#")]
public readonly union IntlMonthStyle(Intl.NumericTwoDigit, Intl.LongShortNarrow)
{
	public Intl.NumericTwoDigit? AsNumeric => Value is Intl.NumericTwoDigit value ? value : default(Intl.NumericTwoDigit?);

	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);
}

[ECMAScript]
[Description("@#")]
public readonly union IntlDurationTextStyle(Intl.LongShortNarrow, Intl.NumericTwoDigit)
{
	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);

	public Intl.NumericTwoDigit? AsNumeric => Value is Intl.NumericTwoDigit value ? value : default(Intl.NumericTwoDigit?);
}

[ECMAScript]
[Description("@#")]
public readonly union IntlDurationFractionStyle(Intl.LongShortNarrow, Intl.DurationNumericStyle)
{
	public Intl.LongShortNarrow? AsText => Value is Intl.LongShortNarrow value ? value : default(Intl.LongShortNarrow?);

	public Intl.DurationNumericStyle? AsNumeric => Value is Intl.DurationNumericStyle value ? value : default(Intl.DurationNumericStyle?);
}
