using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// 时间选择器格式枚举。
/// Time picker format enum.
/// </summary>
[String]
public enum VuetifyTimePickerFormat
{
    [Description("@#ampm")]
    Ampm,

    [Description("@#24hr")]
    TwentyFourHour
}

/// <summary>
/// 时间选择器视图模式枚举。
/// Time picker view mode enum.
/// </summary>
[String]
public enum VuetifyTimePickerViewMode
{
    [Description("@#hour")]
    Hour,

    [Description("@#minute")]
    Minute,

    [Description("@#second")]
    Second
}

/// <summary>
/// 时间选择器上下午时段枚举。
/// Time picker AM/PM period enum.
/// </summary>
[String]
public enum VuetifyTimePickerPeriod
{
    [Description("@#am")]
    Am,

    [Description("@#pm")]
    Pm
}

/// <summary>
/// 时间选择器模型值的擦除值联合类型。
/// Erased value union for time-picker model values.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyTimePickerModelValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Date? _date;

    public VuetifyTimePickerModelValue(string value)
    {
        _kind = 1;
        _string = value;
        _date = default;
    }

    public VuetifyTimePickerModelValue(Date value)
    {
        _kind = 2;
        _string = default;
        _date = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Date? AsDate => _kind == 2 ? _date : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsDate,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTimePickerModelValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTimePickerModelValue From(Date value);

    public static implicit operator VuetifyTimePickerModelValue(string value)
        => new(value);

    public static implicit operator VuetifyTimePickerModelValue(Date value)
        => new(value);
}

/// <summary>
/// 时间选择器允许单元值的解析委托。
/// Delegate for resolving allowed time-picker unit values.
/// </summary>
public delegate bool VuetifyTimePickerAllowedUnitResolver(Number value);

/// <summary>
/// 时间选择器允许单元值列表的擦除值联合类型。
/// Erased value union for time-picker allowed unit value lists.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTimePickerAllowedUnitsCollectionBuilder), nameof(VuetifyTimePickerAllowedUnitsCollectionBuilder.Create))]
public readonly struct VuetifyTimePickerAllowedUnits : System.Runtime.CompilerServices.IUnion, IEnumerable<Number>
{
    private readonly Number[]? _values;

    public VuetifyTimePickerAllowedUnits(Number[] values)
    {
        _values = values;
    }

    public Number[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTimePickerAllowedUnits From(Number[] values);

    public static implicit operator VuetifyTimePickerAllowedUnits(Number[] values)
        => new(values);

    public static implicit operator VuetifyTimePickerAllowedUnits(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyTimePickerAllowedUnits(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTimePickerAllowedUnitsCollectionBuilder
{
    public static VuetifyTimePickerAllowedUnits Create(ReadOnlySpan<Number> values)
        => values.ToArray();
}

/// <summary>
/// 时间选择器允许单元值的擦除值联合类型，支持数组或解析函数。
/// Erased value union for time-picker allowed unit values, supporting arrays or resolver functions.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyTimePickerAllowedUnitValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyTimePickerAllowedUnits? _units;
    private readonly VuetifyTimePickerAllowedUnitResolver? _resolver;

    public VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnits units)
    {
        _kind = 1;
        _units = units;
        _resolver = default;
    }

    public VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnitResolver resolver)
    {
        _kind = 2;
        _units = default;
        _resolver = resolver;
    }

    public VuetifyTimePickerAllowedUnits? AsUnits => _kind == 1 ? _units : default;

    public VuetifyTimePickerAllowedUnitResolver? AsResolver => _kind == 2 ? _resolver : default;

    public object? Value => _kind switch
    {
        1 => AsUnits,
        2 => AsResolver,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTimePickerAllowedUnitValue From(VuetifyTimePickerAllowedUnits units);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyTimePickerAllowedUnitValue From(VuetifyTimePickerAllowedUnitResolver resolver);

    public static implicit operator VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnits units)
        => new(units);

    public static implicit operator VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnitResolver resolver)
        => new(resolver);

    public static implicit operator VuetifyTimePickerAllowedUnitValue(Number[] values)
        => new((VuetifyTimePickerAllowedUnits)values);

    public static implicit operator VuetifyTimePickerAllowedUnitValue(int[] values)
        => new((VuetifyTimePickerAllowedUnits)values);

    public static implicit operator VuetifyTimePickerAllowedUnitValue(double[] values)
        => new((VuetifyTimePickerAllowedUnits)values);
}
