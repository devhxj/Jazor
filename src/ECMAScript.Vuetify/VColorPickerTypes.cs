using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerModesCollectionBuilder), nameof(VuetifyColorPickerModesCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerModes : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyColorPickerMode>
{
    private readonly VuetifyColorPickerMode[]? _modes;

    private VuetifyColorPickerModes(VuetifyColorPickerMode[] modes)
    {
        _modes = modes;
    }

    public VuetifyColorPickerMode[]? AsArray => _modes;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorPickerModes From(VuetifyColorPickerMode[] modes);

    public static implicit operator VuetifyColorPickerModes(VuetifyColorPickerMode[] modes)
        => new(modes);

    IEnumerator<VuetifyColorPickerMode> IEnumerable<VuetifyColorPickerMode>.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerMode>)(_modes ?? [])).GetEnumerator();

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyColorValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly VuetifyRgbColor? _rgb;
    private readonly VuetifyHsvColor? _hsv;
    private readonly VuetifyHslColor? _hsl;

    private VuetifyColorValue(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _rgb = default;
        _hsv = default;
        _hsl = default;
    }

    private VuetifyColorValue(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _rgb = default;
        _hsv = default;
        _hsl = default;
    }

    private VuetifyColorValue(VuetifyRgbColor value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _rgb = value;
        _hsv = default;
        _hsl = default;
    }

    private VuetifyColorValue(VuetifyHsvColor value)
    {
        _kind = 4;
        _string = default;
        _number = default;
        _rgb = default;
        _hsv = value;
        _hsl = default;
    }

    private VuetifyColorValue(VuetifyHslColor value)
    {
        _kind = 5;
        _string = default;
        _number = default;
        _rgb = default;
        _hsv = default;
        _hsl = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public VuetifyRgbColor? AsRgb => _kind == 3 ? _rgb : default;

    public VuetifyHsvColor? AsHsv => _kind == 4 ? _hsv : default;

    public VuetifyHslColor? AsHsl => _kind == 5 ? _hsl : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        3 => AsRgb,
        4 => AsHsv,
        5 => AsHsl,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorValue From(VuetifyRgbColor value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorValue From(VuetifyHsvColor value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorValue From(VuetifyHslColor value);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchCollectionBuilder), nameof(VuetifyColorPickerSwatchCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerSwatch : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyColorValue>
{
    private readonly VuetifyColorValue[]? _colors;

    private VuetifyColorPickerSwatch(VuetifyColorValue[] colors)
    {
        _colors = colors;
    }

    public VuetifyColorValue[]? AsArray => _colors;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorPickerSwatch From(VuetifyColorValue[] colors);

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
        => ((IEnumerable<VuetifyColorValue>)(_colors ?? [])).GetEnumerator();

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchesCollectionBuilder), nameof(VuetifyColorPickerSwatchesCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerSwatches : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyColorPickerSwatch>
{
    private readonly VuetifyColorPickerSwatch[]? _swatches;

    private VuetifyColorPickerSwatches(VuetifyColorPickerSwatch[] swatches)
    {
        _swatches = swatches;
    }

    public VuetifyColorPickerSwatch[]? AsArray => _swatches;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyColorPickerSwatches From(VuetifyColorPickerSwatch[] swatches);

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
        => ((IEnumerable<VuetifyColorPickerSwatch>)(_swatches ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyColorPickerSwatch>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyColorPickerSwatchesCollectionBuilder
{
    public static VuetifyColorPickerSwatches Create(ReadOnlySpan<VuetifyColorPickerSwatch> swatches)
        => swatches.ToArray();
}
