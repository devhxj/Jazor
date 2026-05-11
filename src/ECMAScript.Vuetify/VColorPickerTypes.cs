using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerModesCollectionBuilder), nameof(VuetifyColorPickerModesCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerModes : IEnumerable<VuetifyColorPickerMode>
{
    private readonly VuetifyColorPickerMode[]? _modes;

    private VuetifyColorPickerModes(VuetifyColorPickerMode[] modes)
    {
        _modes = modes;
    }

    public VuetifyColorPickerMode[]? AsArray => _modes;

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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyColorValue
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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchCollectionBuilder), nameof(VuetifyColorPickerSwatchCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerSwatch : IEnumerable<VuetifyColorValue>
{
    private readonly VuetifyColorValue[]? _colors;

    private VuetifyColorPickerSwatch(VuetifyColorValue[] colors)
    {
        _colors = colors;
    }

    public VuetifyColorValue[]? AsArray => _colors;

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

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyColorPickerSwatchesCollectionBuilder), nameof(VuetifyColorPickerSwatchesCollectionBuilder.Create))]
public readonly struct VuetifyColorPickerSwatches : IEnumerable<VuetifyColorPickerSwatch>
{
    private readonly VuetifyColorPickerSwatch[]? _swatches;

    private VuetifyColorPickerSwatches(VuetifyColorPickerSwatch[] swatches)
    {
        _swatches = swatches;
    }

    public VuetifyColorPickerSwatch[]? AsArray => _swatches;

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
