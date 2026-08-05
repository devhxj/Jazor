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
    [Description("@#trend")]
    Trend,

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
    [Description("@#top")]
    Top,

    [Description("@#bottom")]
    Bottom,

    [Description("@#left")]
    Left,

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
    public VuetifySparklineItem[]? AsArray => Value as VuetifySparklineItem[];

    public static implicit operator VuetifySparklineItems(VuetifySparklineItem[] items)
        => new(items);

    public static implicit operator VuetifySparklineItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    public static implicit operator VuetifySparklineItems(Number[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    public static implicit operator VuetifySparklineItems(int[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    public static implicit operator VuetifySparklineItems(double[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    public static implicit operator VuetifySparklineItems(VuetifySparklineValueItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySparklineItem)item));

    IEnumerator<VuetifySparklineItem> IEnumerable<VuetifySparklineItem>.GetEnumerator()
        => ((IEnumerable<VuetifySparklineItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySparklineItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySparklineItemsCollectionBuilder
{
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
    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VuetifySparklineValueItem? AsValueItem => Value as VuetifySparklineValueItem;

    public static implicit operator VuetifySparklineItem(string value)
        => new(value);

    public static implicit operator VuetifySparklineItem(Number value)
        => new(value);

    public static implicit operator VuetifySparklineItem(VuetifySparklineValueItem value)
        => new(value);

    public static implicit operator VuetifySparklineItem(byte value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(short value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(int value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(uint value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(float value)
        => new((Number)value);

    public static implicit operator VuetifySparklineItem(double value)
        => new((Number)value);

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
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public string? AsString => Value as string;

    public static implicit operator VuetifySparklineSmoothValue(bool value)
        => new(value);

    public static implicit operator VuetifySparklineSmoothValue(Number value)
        => new(value);

    public static implicit operator VuetifySparklineSmoothValue(string value)
        => new(value);

    public static implicit operator VuetifySparklineSmoothValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(short value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(int value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(float value)
        => new((Number)value);

    public static implicit operator VuetifySparklineSmoothValue(double value)
        => new((Number)value);

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
    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#value")]
    public string? Value { get; init; }
}
