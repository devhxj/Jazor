using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VColorPicker value domains, collection carriers, and structured color records.
// 定义 VColorPicker 的值域、集合载体和结构化颜色 record；可安全擦除的多值域使用原生 union。

/// <summary>
/// Vuetify 颜色选择器模式枚举。
/// Vuetify color picker mode enum.
/// </summary>
[String]
public enum VuetifyColorPickerMode
{
    /// <summary>
    /// RGB 颜色；上游取值为 “rgb”。
    /// </summary>
    [Description("@#rgb")]
    Rgb,

    /// <summary>
    /// 带透明度的 RGB 颜色；上游取值为 “rgba”。
    /// </summary>
    [Description("@#rgba")]
    Rgba,

    /// <summary>
    /// HSL 颜色；上游取值为 “hsl”。
    /// </summary>
    [Description("@#hsl")]
    Hsl,

    /// <summary>
    /// 带透明度的 HSL 颜色；上游取值为 “hsla”。
    /// </summary>
    [Description("@#hsla")]
    Hsla,

    /// <summary>
    /// 十六进制 RGB 颜色；上游取值为 “hex”。
    /// </summary>
    [Description("@#hex")]
    Hex,

    /// <summary>
    /// 包含透明度的十六进制颜色；上游取值为 “hexa”。
    /// </summary>
    [Description("@#hexa")]
    Hexa
}

/// <summary>
/// Vuetify 颜色选择器可用模式集合。
/// Collection of available Vuetify color picker modes.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerModesCollectionBuilder), nameof(VuetifyColorPickerModesCollectionBuilder.Create))]
public readonly union VuetifyColorPickerModes(VuetifyColorPickerMode[]) : IEnumerable<VuetifyColorPickerMode>
{
    /// <summary>
    /// 读取当前值的 VuetifyColorPickerMode[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyColorPickerMode[]? AsArray => Value as VuetifyColorPickerMode[];

    /// <summary>
    /// 将 VuetifyColorPickerMode[] 值转换为 VuetifyColorPickerModes，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerModes(VuetifyColorPickerMode[] modes)
        => new(modes);

    IEnumerator<VuetifyColorPickerMode> IEnumerable<VuetifyColorPickerMode>.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerMode>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerMode>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerModesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyColorPickerModes Create(ReadOnlySpan<VuetifyColorPickerMode> modes)
        => modes.ToArray();
}

/// <summary>
/// Vuetify RGB 颜色值。
/// Vuetify RGB color value.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyRgbColor : VueProps
{
    /// <summary>
    /// RGB 红色通道，取值为 0 到 255。
    /// </summary>
    [Description("@#r")]
    public Number? R { get; init; }

    /// <summary>
    /// RGB 绿色通道，取值为 0 到 255。
    /// </summary>
    [Description("@#g")]
    public Number? G { get; init; }

    /// <summary>
    /// RGB 蓝色通道，取值为 0 到 255。
    /// </summary>
    [Description("@#b")]
    public Number? B { get; init; }

    /// <summary>
    /// 颜色透明度 alpha，取值通常为 0 到 1。
    /// </summary>
    [Description("@#a")]
    public Number? A { get; init; }
}

/// <summary>
/// Vuetify HSV 颜色值。
/// Vuetify HSV color value.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyHsvColor : VueProps
{
    /// <summary>
    /// 色相角度，单位为度。
    /// </summary>
    [Description("@#h")]
    public Number? H { get; init; }

    /// <summary>
    /// 颜色饱和度，按 Vuetify 颜色对象使用 0 到 1 的比例。
    /// </summary>
    [Description("@#s")]
    public Number? S { get; init; }

    /// <summary>
    /// HSV 明度，取值为 0 到 1。
    /// </summary>
    [Description("@#v")]
    public Number? V { get; init; }

    /// <summary>
    /// 颜色透明度 alpha，取值通常为 0 到 1。
    /// </summary>
    [Description("@#a")]
    public Number? A { get; init; }
}

/// <summary>
/// Vuetify HSL 颜色值。
/// Vuetify HSL color value.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyHslColor : VueProps
{
    /// <summary>
    /// 色相角度，单位为度。
    /// </summary>
    [Description("@#h")]
    public Number? H { get; init; }

    /// <summary>
    /// 颜色饱和度，按 Vuetify 颜色对象使用 0 到 1 的比例。
    /// </summary>
    [Description("@#s")]
    public Number? S { get; init; }

    /// <summary>
    /// HSL 亮度，取值为 0 到 1。
    /// </summary>
    [Description("@#l")]
    public Number? L { get; init; }

    /// <summary>
    /// 颜色透明度 alpha，取值通常为 0 到 1。
    /// </summary>
    [Description("@#a")]
    public Number? A { get; init; }
}

/// <summary>
/// Vuetify 颜色值联合类型，支持字符串、数值、RGB、HSV 和 HSL 表示。
/// Vuetify color value union type supporting string, number, RGB, HSV, and HSL representations.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyColorValue(string, Number, VuetifyRgbColor, VuetifyHsvColor, VuetifyHslColor)
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
    /// 读取当前值的 VuetifyRgbColor 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyRgbColor? AsRgb => Value as VuetifyRgbColor;

    /// <summary>
    /// 读取当前值的 VuetifyHsvColor 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyHsvColor? AsHsv => Value as VuetifyHsvColor;

    /// <summary>
    /// 读取当前值的 VuetifyHslColor 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyHslColor? AsHsl => Value as VuetifyHslColor;

    /// <summary>
    /// 将 string 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyRgbColor 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(VuetifyRgbColor value)
        => new(value);

    /// <summary>
    /// 将 VuetifyHsvColor 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(VuetifyHsvColor value)
        => new(value);

    /// <summary>
    /// 将 VuetifyHslColor 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(VuetifyHslColor value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyColorValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify 颜色选择器色板行，表示一组颜色值。
/// Vuetify color picker swatch row, representing a group of color values.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchCollectionBuilder), nameof(VuetifyColorPickerSwatchCollectionBuilder.Create))]
public readonly union VuetifyColorPickerSwatch(VuetifyColorValue[]) : IEnumerable<VuetifyColorValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyColorValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyColorValue[]? AsArray => Value as VuetifyColorValue[];

    /// <summary>
    /// 将 VuetifyColorValue[] 值转换为 VuetifyColorPickerSwatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(VuetifyColorValue[] colors)
        => new(colors);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(string[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(Number[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(int[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(double[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 VuetifyRgbColor[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(VuetifyRgbColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 VuetifyHsvColor[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(VuetifyHsvColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    /// <summary>
    /// 将 VuetifyHslColor[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatch(VuetifyHslColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    IEnumerator<VuetifyColorValue> IEnumerable<VuetifyColorValue>.GetEnumerator()
        => ((IEnumerable<VuetifyColorValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerSwatchCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyColorPickerSwatch Create(ReadOnlySpan<VuetifyColorValue> colors)
        => colors.ToArray();
}

/// <summary>
/// Vuetify 颜色选择器色板集合。
/// Vuetify color picker swatches collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchesCollectionBuilder), nameof(VuetifyColorPickerSwatchesCollectionBuilder.Create))]
public readonly union VuetifyColorPickerSwatches(VuetifyColorPickerSwatch[]) : IEnumerable<VuetifyColorPickerSwatch>
{
    /// <summary>
    /// 读取当前值的 VuetifyColorPickerSwatch[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyColorPickerSwatch[]? AsArray => Value as VuetifyColorPickerSwatch[];

    /// <summary>
    /// 将 VuetifyColorPickerSwatch[] 值转换为 VuetifyColorPickerSwatches，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatches(VuetifyColorPickerSwatch[] swatches)
        => new(swatches);

    /// <summary>
    /// 将 string[][] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatches(string[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    /// <summary>
    /// 将 Number[][] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatches(Number[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    /// <summary>
    /// 将 int[][] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatches(int[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    /// <summary>
    /// 将 double[][] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyColorPickerSwatches(double[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    IEnumerator<VuetifyColorPickerSwatch> IEnumerable<VuetifyColorPickerSwatch>.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerSwatch>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerSwatch>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerSwatchesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyColorPickerSwatches Create(ReadOnlySpan<VuetifyColorPickerSwatch> swatches)
        => swatches.ToArray();
}