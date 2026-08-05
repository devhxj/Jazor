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
    [Description("@#rgb")]
    Rgb,

    [Description("@#rgba")]
    Rgba,

    [Description("@#hsl")]
    Hsl,

    [Description("@#hsla")]
    Hsla,

    [Description("@#hex")]
    Hex,

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
    public VuetifyColorPickerMode[]? AsArray => Value as VuetifyColorPickerMode[];

    public static implicit operator VuetifyColorPickerModes(VuetifyColorPickerMode[] modes)
        => new(modes);

    IEnumerator<VuetifyColorPickerMode> IEnumerable<VuetifyColorPickerMode>.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerMode>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerMode>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerModesCollectionBuilder
{
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
    [Description("@#r")]
    public Number? R { get; init; }

    [Description("@#g")]
    public Number? G { get; init; }

    [Description("@#b")]
    public Number? B { get; init; }

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
    [Description("@#h")]
    public Number? H { get; init; }

    [Description("@#s")]
    public Number? S { get; init; }

    [Description("@#v")]
    public Number? V { get; init; }

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
    [Description("@#h")]
    public Number? H { get; init; }

    [Description("@#s")]
    public Number? S { get; init; }

    [Description("@#l")]
    public Number? L { get; init; }

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
    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VuetifyRgbColor? AsRgb => Value as VuetifyRgbColor;

    public VuetifyHsvColor? AsHsv => Value as VuetifyHsvColor;

    public VuetifyHslColor? AsHsl => Value as VuetifyHslColor;

    public static implicit operator VuetifyColorValue(string value)
        => new(value);

    public static implicit operator VuetifyColorValue(Number value)
        => new(value);

    public static implicit operator VuetifyColorValue(VuetifyRgbColor value)
        => new(value);

    public static implicit operator VuetifyColorValue(VuetifyHsvColor value)
        => new(value);

    public static implicit operator VuetifyColorValue(VuetifyHslColor value)
        => new(value);

    public static implicit operator VuetifyColorValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyColorValue(double value)
        => new((Number)value);

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
    public VuetifyColorValue[]? AsArray => Value as VuetifyColorValue[];

    public static implicit operator VuetifyColorPickerSwatch(VuetifyColorValue[] colors)
        => new(colors);

    public static implicit operator VuetifyColorPickerSwatch(string[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(Number[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(int[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(double[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(VuetifyRgbColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(VuetifyHsvColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    public static implicit operator VuetifyColorPickerSwatch(VuetifyHslColor[] colors)
        => new(Array.ConvertAll(colors, static color => (VuetifyColorValue)color));

    IEnumerator<VuetifyColorValue> IEnumerable<VuetifyColorValue>.GetEnumerator()
        => ((IEnumerable<VuetifyColorValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerSwatchCollectionBuilder
{
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
    public VuetifyColorPickerSwatch[]? AsArray => Value as VuetifyColorPickerSwatch[];

    public static implicit operator VuetifyColorPickerSwatches(VuetifyColorPickerSwatch[] swatches)
        => new(swatches);

    public static implicit operator VuetifyColorPickerSwatches(string[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    public static implicit operator VuetifyColorPickerSwatches(Number[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    public static implicit operator VuetifyColorPickerSwatches(int[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    public static implicit operator VuetifyColorPickerSwatches(double[][] swatches)
        => new(Array.ConvertAll(swatches, static swatch => (VuetifyColorPickerSwatch)swatch));

    IEnumerator<VuetifyColorPickerSwatch> IEnumerable<VuetifyColorPickerSwatch>.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerSwatch>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerSwatch>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerSwatchesCollectionBuilder
{
    public static VuetifyColorPickerSwatches Create(ReadOnlySpan<VuetifyColorPickerSwatch> swatches)
        => swatches.ToArray();
}
