using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// 迷你图表的类型枚举。
/// Sparkline visualization type enum.
/// </summary>
[String]
public enum VuetifySparklineType
{
    /// <summary>
    /// 以趋势折线绘制数据；上游取值为 “trend”。
    /// </summary>
    [Description("@#trend")]
    Trend,

    /// <summary>
    /// 以条形绘制数据；上游取值为 “bar”。
    /// </summary>
    [Description("@#bar")]
    Bar
}

/// <summary>
/// 迷你图表渐变方向枚举。
/// Sparkline gradient direction enum.
/// </summary>
[String]
public enum VuetifySparklineGradientDirection
{
    /// <summary>
    /// 顶部；上游取值为 “top”。
    /// </summary>
    [Description("@#top")]
    Top,

    /// <summary>
    /// 底部；上游取值为 “bottom”。
    /// </summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
    [Description("@#right")]
    Right
}

/// <summary>
/// 迷你图表数据项列表的擦除值联合类型。
/// Erased value union for sparkline data item lists.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySparklineItemsCollectionBuilder), nameof(VuetifySparklineItemsCollectionBuilder.Create))]
public readonly union VuetifySparklineItems(VuetifySparklineItem[]) : IEnumerable<VuetifySparklineItem>
{
    /// <summary>
    /// 读取当前值的 VuetifySparklineItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySparklineItem[]? AsArray => Value as VuetifySparklineItem[];

    /// <summary>
    /// 将 VuetifySparklineItem[] 值转换为 VuetifySparklineItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(VuetifySparklineItem[] items)
        => new(items);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(Number[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(int[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(double[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    /// <summary>
    /// 将 VuetifySparklineValueItem[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItems(VuetifySparklineValueItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    IEnumerator<VuetifySparklineItem> IEnumerable<VuetifySparklineItem>.GetEnumerator()
        => ((IEnumerable<VuetifySparklineItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySparklineItem>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySparklineItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifySparklineItems Create(ReadOnlySpan<VuetifySparklineItem> items)
        => items.ToArray();
}

/// <summary>
/// 单个迷你图表数据项的擦除值联合类型。
/// Erased value union for a single sparkline data item.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySparklineItem(string, Number, VuetifySparklineValueItem)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VuetifySparklineValueItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySparklineValueItem? AsValueItem => Value as VuetifySparklineValueItem;

    /// <summary>
    /// 将 string 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifySparklineValueItem 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(VuetifySparklineValueItem value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifySparklineItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineItem(decimal value)
        => new((Number)value);
}

/// <summary>
/// 带有显式值的迷你图表数据项记录。
/// Sparkline data item record with an explicit value.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifySparklineValueItem : VueProps
{
    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public Number? Value { get; init; }
}

/// <summary>
/// 迷你图表平滑度的擦除值联合类型。
/// Erased value union for sparkline smooth setting.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySparklineSmoothValue(bool, Number, string)
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 将 bool 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(Number value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifySparklineSmoothValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySparklineSmoothValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify VSparkline 标签插槽所暴露的插槽上下文。
/// Label slot context exposed by Vuetify VSparkline.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSparklineLabelSlotContext
{
    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public string? Value { get; init; }
}