using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 日期选择器视图模式。
/// Vuetify date-picker view mode.
/// </summary>
[String]
public enum VuetifyDatePickerViewMode
{
    [Description("@#month")]
    Month,

    [Description("@#months")]
    Months,

    [Description("@#year")]
    Year
}

/// <summary>
/// Vuetify 日期选择器每月周数模式。
/// Vuetify date-picker weeks-in-month mode.
/// </summary>
[String]
public enum VuetifyDatePickerWeeksInMonth
{
    [Description("@#static")]
    Static,

    [Description("@#dynamic")]
    Dynamic
}

/// <summary>
/// Vuetify 日期选择器多选模式。
/// Vuetify date-picker multiple-selection mode.
/// </summary>
[String]
public enum VuetifyDatePickerMultipleMode
{
    [Description("@#range")]
    Range
}

/// <summary>
/// Vuetify 日历星期枚举。
/// Vuetify calendar weekday enumeration.
/// </summary>
public enum VuetifyCalendarWeekday
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

/// <summary>
/// Vuetify 日历工作日集合。
/// Vuetify calendar weekdays collection.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarWeekdaysCollectionBuilder), nameof(VuetifyCalendarWeekdaysCollectionBuilder.Create))]
public readonly struct VuetifyCalendarWeekdays : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyCalendarWeekday>
{
    private readonly VuetifyCalendarWeekday[]? _weekdays;

    public VuetifyCalendarWeekdays(VuetifyCalendarWeekday[] weekdays)
    {
        _weekdays = weekdays;
    }

    public VuetifyCalendarWeekday[]? AsArray => _weekdays;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarWeekdays From(VuetifyCalendarWeekday[] weekdays);

    public static implicit operator VuetifyCalendarWeekdays(VuetifyCalendarWeekday[] weekdays)
        => new(weekdays);

    IEnumerator<VuetifyCalendarWeekday> IEnumerable<VuetifyCalendarWeekday>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)(_weekdays ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarWeekdaysCollectionBuilder
{
    public static VuetifyCalendarWeekdays Create(ReadOnlySpan<VuetifyCalendarWeekday> weekdays)
        => weekdays.ToArray();
}

/// <summary>
/// Vuetify 日期选择器多选值。
/// Vuetify date-picker multiple-selection value.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDatePickerMultipleValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly VuetifyDatePickerMultipleMode? _mode;
    private readonly string? _customMode;

    public VuetifyDatePickerMultipleValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _mode = default;
        _customMode = default;
    }

    public VuetifyDatePickerMultipleValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _mode = default;
        _customMode = default;
    }

    public VuetifyDatePickerMultipleValue(VuetifyDatePickerMultipleMode value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _mode = value;
        _customMode = default;
    }

    public VuetifyDatePickerMultipleValue(string value)
    {
        _kind = 4;
        _bool = default;
        _number = default;
        _mode = default;
        _customMode = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public VuetifyDatePickerMultipleMode? AsMode => _kind == 3 ? _mode : default;

    public string? AsCustomMode => _kind == 4 ? _customMode : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsMode,
        4 => AsCustomMode,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerMultipleValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerMultipleValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerMultipleValue From(VuetifyDatePickerMultipleMode value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerMultipleValue From(string value);

    public static implicit operator VuetifyDatePickerMultipleValue(bool value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(Number value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(VuetifyDatePickerMultipleMode value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify 日期选择器模型值集合。
/// Vuetify date-picker model value collection.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDatePickerModelValuesCollectionBuilder), nameof(VuetifyDatePickerModelValuesCollectionBuilder.Create))]
public readonly struct VuetifyDatePickerModelValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VueValue>
{
    private readonly VueValue[]? _values;

    public VuetifyDatePickerModelValues(VueValue[] values)
    {
        _values = values;
    }

    public VueValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerModelValues From(VueValue[] values);

    public static implicit operator VuetifyDatePickerModelValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDatePickerModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerModelValuesCollectionBuilder
{
    public static VuetifyDatePickerModelValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 日期选择器模型值。
/// Vuetify date-picker model value.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDatePickerModelValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Date? _date;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly VuetifyDatePickerModelValues? _values;

    public VuetifyDatePickerModelValue(Date value)
    {
        _kind = 1;
        _date = value;
        _string = default;
        _number = default;
        _values = default;
    }

    public VuetifyDatePickerModelValue(string value)
    {
        _kind = 2;
        _date = default;
        _string = value;
        _number = default;
        _values = default;
    }

    public VuetifyDatePickerModelValue(Number value)
    {
        _kind = 3;
        _date = default;
        _string = default;
        _number = value;
        _values = default;
    }

    public VuetifyDatePickerModelValue(VuetifyDatePickerModelValues value)
    {
        _kind = 4;
        _date = default;
        _string = default;
        _number = default;
        _values = value;
    }

    public Date? AsDate => _kind == 1 ? _date : default;

    public string? AsString => _kind == 2 ? _string : default;

    public Number? AsNumber => _kind == 3 ? _number : default;

    public VuetifyDatePickerModelValues? AsValues => _kind == 4 ? _values : default;

    public object? Value => _kind switch
    {
        1 => AsDate,
        2 => AsString,
        3 => AsNumber,
        4 => AsValues,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerModelValue From(Date value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerModelValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerModelValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerModelValue From(VuetifyDatePickerModelValues value);

    public static implicit operator VuetifyDatePickerModelValue(Date value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(Number value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(VuetifyDatePickerModelValues value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(VueValue[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(string[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(Number[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(int[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(double[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(decimal value)
        => new((Number)value);
}

public delegate bool VuetifyDatePickerAllowedDateResolver(VueValue? date);

/// <summary>
/// Vuetify 日期选择器允许日期集合。
/// Vuetify date-picker allowed-dates collection.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDatePickerAllowedDatesCollectionBuilder), nameof(VuetifyDatePickerAllowedDatesCollectionBuilder.Create))]
public readonly struct VuetifyDatePickerAllowedDates : System.Runtime.CompilerServices.IUnion, IEnumerable<VueValue>
{
    private readonly VueValue[]? _values;

    public VuetifyDatePickerAllowedDates(VueValue[] values)
    {
        _values = values;
    }

    public VueValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerAllowedDates From(VueValue[] values);

    public static implicit operator VuetifyDatePickerAllowedDates(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDatePickerAllowedDates(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerAllowedDatesCollectionBuilder
{
    public static VuetifyDatePickerAllowedDates Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 日期选择器允许日期值。
/// Vuetify date-picker allowed-dates value.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDatePickerAllowedDatesValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyDatePickerAllowedDates? _dates;
    private readonly VuetifyDatePickerAllowedDateResolver? _resolver;

    public VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDates dates)
    {
        _kind = 1;
        _dates = dates;
        _resolver = default;
    }

    public VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDateResolver resolver)
    {
        _kind = 2;
        _dates = default;
        _resolver = resolver;
    }

    public VuetifyDatePickerAllowedDates? AsDates => _kind == 1 ? _dates : default;

    public VuetifyDatePickerAllowedDateResolver? AsResolver => _kind == 2 ? _resolver : default;

    public object? Value => _kind switch
    {
        1 => AsDates,
        2 => AsResolver,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerAllowedDatesValue From(VuetifyDatePickerAllowedDates dates);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerAllowedDatesValue From(VuetifyDatePickerAllowedDateResolver resolver);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDates dates)
        => new(dates);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDateResolver resolver)
        => new(resolver);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VueValue[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(string[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(Number[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(int[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(double[] values)
        => new((VuetifyDatePickerAllowedDates)values);
}

/// <summary>
/// Vuetify 日期选择器激活值。
/// Vuetify date-picker active value.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyDatePickerActiveValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _active;
    private readonly string[]? _activeValues;

    public VuetifyDatePickerActiveValue(string value)
    {
        _kind = 1;
        _active = value;
        _activeValues = default;
    }

    public VuetifyDatePickerActiveValue(string[] value)
    {
        _kind = 2;
        _active = default;
        _activeValues = value;
    }

    public string? AsString => _kind == 1 ? _active : default;

    public string[]? AsStrings => _kind == 2 ? _activeValues : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsStrings,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerActiveValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDatePickerActiveValue From(string[] value);

    public static implicit operator VuetifyDatePickerActiveValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerActiveValue(string[] value)
        => new(value);
}

/// <summary>
/// Vuetify VDatePicker 标题插槽上下文。
/// Vuetify VDatePicker header slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDatePickerHeaderSlotContext
{
    [Description("@#header")]
    public string? Header { get; init; }

    [Description("@#transition")]
    public string? Transition { get; init; }
}
