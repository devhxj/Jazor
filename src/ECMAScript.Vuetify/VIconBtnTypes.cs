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
    /// <summary>
    /// 特小尺寸；上游取值为 “x-small”。
    /// </summary>
    [Description("@#x-small")]
    XSmall,

    /// <summary>
    /// 小号尺寸；上游取值为 “small”。
    /// </summary>
    [Description("@#small")]
    Small,

    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 大号尺寸；上游取值为 “large”。
    /// </summary>
    [Description("@#large")]
    Large,

    /// <summary>
    /// 特大尺寸；上游取值为 “x-large”。
    /// </summary>
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
    /// <summary>
    /// 尺寸条目的名称，用于选择对应的按钮尺寸配置。
    /// </summary>
    [Description("@#name")]
    public VIconBtnSizeName Name { get; init; }

    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 VIconBtnSizeEntry[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VIconBtnSizeEntry[]? AsArray => Value as VIconBtnSizeEntry[];

    /// <summary>
    /// 将 VIconBtnSizeEntry[] 值转换为 VIconBtnSizeMap，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnSizeMap(VIconBtnSizeEntry[] entries)
        => new(entries);

    IEnumerator<VIconBtnSizeEntry> IEnumerable<VIconBtnSizeEntry>.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VIconBtnSizeEntry>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VIconBtnSizeMapCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
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
    /// 将 bool 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(Number value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VIconBtnTextValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VIconBtnTextValue(decimal value)
        => new((Number)value);
}