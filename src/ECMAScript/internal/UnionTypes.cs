using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PropertyKey
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Number? _number;
	private readonly Symbol? _symbol;

	private PropertyKey(string value)
	{
		_kind = 1;
		_string = value;
		_number = default;
		_symbol = default;
	}

	private PropertyKey(Number value)
	{
		_kind = 2;
		_string = default;
		_number = value;
		_symbol = default;
	}

	private PropertyKey(Symbol value)
	{
		_kind = 3;
		_string = default;
		_number = default;
		_symbol = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Number? AsNumber => _kind == 2 ? _number : default;

	public Symbol? AsSymbol => _kind == 3 ? _symbol : default;

	public static implicit operator PropertyKey(string value)
		=> new(value);

	public static implicit operator PropertyKey(Number value)
		=> new(value);

	public static implicit operator PropertyKey(Symbol value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DatePrimitive
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Number? _number;

	private DatePrimitive(string value)
	{
		_kind = 1;
		_string = value;
		_number = default;
	}

	private DatePrimitive(Number value)
	{
		_kind = 2;
		_string = default;
		_number = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Number? AsNumber => _kind == 2 ? _number : default;

	public static implicit operator DatePrimitive(string value)
		=> new(value);

	public static implicit operator DatePrimitive(Number value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AtomicsWaitAsyncValue
{
	private readonly byte _kind;
	private readonly string? _string;
	private readonly Promise<string>? _promise;

	private AtomicsWaitAsyncValue(string value)
	{
		_kind = 1;
		_string = value;
		_promise = default;
	}

	private AtomicsWaitAsyncValue(Promise<string> value)
	{
		_kind = 2;
		_string = default;
		_promise = value;
	}

	public string? AsString => _kind == 1 ? _string : default;

	public Promise<string>? AsPromise => _kind == 2 ? _promise : default;

	public static implicit operator AtomicsWaitAsyncValue(string value)
		=> new(value);

	public static implicit operator AtomicsWaitAsyncValue(Promise<string> value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlUseGrouping
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly Intl.NumberFormatUseGrouping? _mode;

	private IntlUseGrouping(bool value)
	{
		_kind = 1;
		_bool = value;
		_mode = default;
	}

	private IntlUseGrouping(Intl.NumberFormatUseGrouping value)
	{
		_kind = 2;
		_bool = default;
		_mode = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public Intl.NumberFormatUseGrouping? AsMode => _kind == 2 ? _mode : default;

	public static implicit operator IntlUseGrouping(bool value)
		=> new(value);

	public static implicit operator IntlUseGrouping(Intl.NumberFormatUseGrouping value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlNumberInput
{
	private readonly byte _kind;
	private readonly Number? _number;
	private readonly BigInt? _bigInt;
	private readonly string? _string;

	private IntlNumberInput(Number value)
	{
		_kind = 1;
		_number = value;
		_bigInt = default;
		_string = default;
	}

	private IntlNumberInput(BigInt value)
	{
		_kind = 2;
		_number = default;
		_bigInt = value;
		_string = default;
	}

	private IntlNumberInput(string value)
	{
		_kind = 3;
		_number = default;
		_bigInt = default;
		_string = value;
	}

	public Number? AsNumber => _kind == 1 ? _number : default;

	public BigInt? AsBigInt => _kind == 2 ? _bigInt : default;

	public string? AsString => _kind == 3 ? _string : default;

	public static implicit operator IntlNumberInput(Number value)
		=> new(value);

	public static implicit operator IntlNumberInput(BigInt value)
		=> new(value);

	public static implicit operator IntlNumberInput(string value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlDateTimeInput
{
	private readonly byte _kind;
	private readonly Date? _date;
	private readonly Number? _number;

	private IntlDateTimeInput(Date value)
	{
		_kind = 1;
		_date = value;
		_number = default;
	}

	private IntlDateTimeInput(Number value)
	{
		_kind = 2;
		_date = default;
		_number = value;
	}

	public Date? AsDate => _kind == 1 ? _date : default;

	public Number? AsNumber => _kind == 2 ? _number : default;

	public static implicit operator IntlDateTimeInput(Date value)
		=> new(value);

	public static implicit operator IntlDateTimeInput(Number value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlMonthStyle
{
	private readonly byte _kind;
	private readonly Intl.NumericTwoDigit? _numeric;
	private readonly Intl.LongShortNarrow? _text;

	private IntlMonthStyle(Intl.NumericTwoDigit value)
	{
		_kind = 1;
		_numeric = value;
		_text = default;
	}

	private IntlMonthStyle(Intl.LongShortNarrow value)
	{
		_kind = 2;
		_numeric = default;
		_text = value;
	}

	public Intl.NumericTwoDigit? AsNumeric => _kind == 1 ? _numeric : default;

	public Intl.LongShortNarrow? AsText => _kind == 2 ? _text : default;

	public static implicit operator IntlMonthStyle(Intl.NumericTwoDigit value)
		=> new(value);

	public static implicit operator IntlMonthStyle(Intl.LongShortNarrow value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlDurationTextStyle
{
	private readonly byte _kind;
	private readonly Intl.LongShortNarrow? _text;
	private readonly Intl.NumericTwoDigit? _numeric;

	private IntlDurationTextStyle(Intl.LongShortNarrow value)
	{
		_kind = 1;
		_text = value;
		_numeric = default;
	}

	private IntlDurationTextStyle(Intl.NumericTwoDigit value)
	{
		_kind = 2;
		_text = default;
		_numeric = value;
	}

	public Intl.LongShortNarrow? AsText => _kind == 1 ? _text : default;

	public Intl.NumericTwoDigit? AsNumeric => _kind == 2 ? _numeric : default;

	public static implicit operator IntlDurationTextStyle(Intl.LongShortNarrow value)
		=> new(value);

	public static implicit operator IntlDurationTextStyle(Intl.NumericTwoDigit value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntlDurationFractionStyle
{
	private readonly byte _kind;
	private readonly Intl.LongShortNarrow? _text;
	private readonly Intl.DurationNumericStyle? _numeric;

	private IntlDurationFractionStyle(Intl.LongShortNarrow value)
	{
		_kind = 1;
		_text = value;
		_numeric = default;
	}

	private IntlDurationFractionStyle(Intl.DurationNumericStyle value)
	{
		_kind = 2;
		_text = default;
		_numeric = value;
	}

	public Intl.LongShortNarrow? AsText => _kind == 1 ? _text : default;

	public Intl.DurationNumericStyle? AsNumeric => _kind == 2 ? _numeric : default;

	public static implicit operator IntlDurationFractionStyle(Intl.LongShortNarrow value)
		=> new(value);

	public static implicit operator IntlDurationFractionStyle(Intl.DurationNumericStyle value)
		=> new(value);
}
