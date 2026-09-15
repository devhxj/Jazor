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
    /// <summary>
    /// 使用带上午/下午标记的 12 小时制；上游取值为 “ampm”。
    /// </summary>
    [Description("@#ampm")]
    Ampm,

    /// <summary>
    /// 使用 24 小时制；上游取值为 “24hr”。
    /// </summary>
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
    /// <summary>
    /// 选择小时；上游取值为 “hour”。
    /// </summary>
    [Description("@#hour")]
    Hour,

    /// <summary>
    /// 选择分钟；上游取值为 “minute”。
    /// </summary>
    [Description("@#minute")]
    Minute,

    /// <summary>
    /// 选择秒；上游取值为 “second”。
    /// </summary>
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
    /// <summary>
    /// 上午时段；上游取值为 “am”。
    /// </summary>
    [Description("@#am")]
    Am,

    /// <summary>
    /// 下午时段；上游取值为 “pm”。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Date 分支；不属于该分支时返回 null。
    /// </summary>
    public Date? AsDate => Value is Date value ? value : default(Date?);

    /// <summary>
    /// 将 string 值转换为 VuetifyTimePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerModelValue(string value)
        => new(value);

    /// <summary>
    /// 将 Date 值转换为 VuetifyTimePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsArray => Value as Number[];

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyTimePickerAllowedUnits，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnits(Number[] values)
        => new(values);

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnits(int[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnits(double[] values)
        => new(Array.ConvertAll(values, static value => (Number)value));

    IEnumerator<Number> IEnumerable<Number>.GetEnumerator()
        => ((IEnumerable<Number>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Number>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTimePickerAllowedUnitsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
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
    /// <summary>
    /// 读取当前值的 VuetifyTimePickerAllowedUnits 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTimePickerAllowedUnits? AsUnits
        => Value is VuetifyTimePickerAllowedUnits value ? value : default(VuetifyTimePickerAllowedUnits?);

    /// <summary>
    /// 读取当前值的 VuetifyTimePickerAllowedUnitResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTimePickerAllowedUnitResolver? AsResolver => Value as VuetifyTimePickerAllowedUnitResolver;

    /// <summary>
    /// 将 VuetifyTimePickerAllowedUnits 值转换为 VuetifyTimePickerAllowedUnitValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnits units)
        => new(units);

    /// <summary>
    /// 将 VuetifyTimePickerAllowedUnitResolver 值转换为 VuetifyTimePickerAllowedUnitValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnitValue(VuetifyTimePickerAllowedUnitResolver resolver)
        => new(resolver);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyTimePickerAllowedUnitValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnitValue(Number[] values)
        => new((VuetifyTimePickerAllowedUnits)values);

    /// <summary>
    /// 将 int[] 值转换为 VuetifyTimePickerAllowedUnitValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnitValue(int[] values)
        => new((VuetifyTimePickerAllowedUnits)values);

    /// <summary>
    /// 将 double[] 值转换为 VuetifyTimePickerAllowedUnitValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTimePickerAllowedUnitValue(double[] values)
        => new((VuetifyTimePickerAllowedUnits)values);
}