using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VTimePicker model and allowed-unit value contracts.
// 定义 VTimePicker 的模型和允许单位值合同；可擦除的多值域使用原生 union。

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
[Description("@#")]
public readonly union VuetifyTimePickerModelValue(string, Date)
{
    public string? AsString => Value as string;

    public Date? AsDate => Value is Date value ? value : default(Date?);

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
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTimePickerAllowedUnitsCollectionBuilder), nameof(VuetifyTimePickerAllowedUnitsCollectionBuilder.Create))]
public readonly union VuetifyTimePickerAllowedUnits(Number[]) : IEnumerable<Number>
{
    public Number[]? AsArray => Value as Number[];

    public static implicit operator VuetifyTimePickerAllowedUnits(Number[] values)
        => new(values);

    public static implicit operator VuetifyTimePickerAllowedUnits(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    public static implicit operator VuetifyTimePickerAllowedUnits(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsArray ?? [])).GetEnumerator();

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
[Description("@#")]
public readonly union VuetifyTimePickerAllowedUnitValue(
    VuetifyTimePickerAllowedUnits,
    VuetifyTimePickerAllowedUnitResolver)
{
    public VuetifyTimePickerAllowedUnits? AsUnits
        => Value is VuetifyTimePickerAllowedUnits value ? value : default(VuetifyTimePickerAllowedUnits?);

    public VuetifyTimePickerAllowedUnitResolver? AsResolver => Value as VuetifyTimePickerAllowedUnitResolver;

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
