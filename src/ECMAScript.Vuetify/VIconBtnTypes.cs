using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VIconBtn size-map and text value contracts for RazorVue authoring.
// 定义 VIconBtn 的尺寸映射和文本值合同；可安全擦除的多值域使用原生 union。

/// <summary>
/// 图标按钮尺寸名称枚举。
/// Icon button size name enumeration.
/// </summary>
[String]
public enum VIconBtnSizeName
{
    [Description("@#x-small")]
    XSmall,

    [Description("@#small")]
    Small,

    [Description("@#default")]
    Default,

    [Description("@#large")]
    Large,

    [Description("@#x-large")]
    XLarge
}

/// <summary>
/// 图标按钮尺寸条目，包含名称和数值。
/// Icon button size entry containing a name and value pair.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VIconBtnSizeEntry
{
    [Description("@#name")]
    public VIconBtnSizeName Name { get; init; }

    [Description("@#value")]
    public Number Value { get; init; }
}

/// <summary>
/// 图标按钮尺寸映射表，将尺寸名称映射到具体数值。
/// Icon button size map that maps size names to numeric values.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VIconBtnSizeMapCollectionBuilder), nameof(VIconBtnSizeMapCollectionBuilder.Create))]
public readonly union VIconBtnSizeMap(VIconBtnSizeEntry[]) : IEnumerable<VIconBtnSizeEntry>
{
    public VIconBtnSizeEntry[]? AsArray => Value as VIconBtnSizeEntry[];

    public static implicit operator VIconBtnSizeMap(VIconBtnSizeEntry[] entries)
        => new(entries);

    IEnumerator<VIconBtnSizeEntry> IEnumerable<VIconBtnSizeEntry>.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VIconBtnSizeMapCollectionBuilder
{
    public static VIconBtnSizeMap Create(ReadOnlySpan<VIconBtnSizeEntry> entries)
        => entries.ToArray();
}

/// <summary>
/// 图标按钮文本值，支持布尔、数字或字符串类型。
/// Icon button text value supporting boolean, numeric, or string types.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VIconBtnTextValue(bool, Number, string)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public string? AsString => Value as string;

    public static implicit operator VIconBtnTextValue(bool value)
        => new(value);

    public static implicit operator VIconBtnTextValue(Number value)
        => new(value);

    public static implicit operator VIconBtnTextValue(string value)
        => new(value);

    public static implicit operator VIconBtnTextValue(byte value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(sbyte value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(short value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(ushort value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(int value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(uint value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(float value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(double value)
        => new((Number)value);

    public static implicit operator VIconBtnTextValue(decimal value)
        => new((Number)value);
}
