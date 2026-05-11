using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

[String]
public enum VuetifySparklineType
{
    [Description("@#trend")]
    Trend,

    [Description("@#bar")]
    Bar
}

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

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySparklineItemsCollectionBuilder), nameof(VuetifySparklineItemsCollectionBuilder.Create))]
public readonly struct VuetifySparklineItems : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifySparklineItem>
{
    private readonly VuetifySparklineItem[]? _items;

    private VuetifySparklineItems(VuetifySparklineItem[] items)
    {
        _items = items;
    }

    public VuetifySparklineItem[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineItems From(VuetifySparklineItem[] items);

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
        => ((IEnumerable<VuetifySparklineItem>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySparklineItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySparklineItemsCollectionBuilder
{
    public static VuetifySparklineItems Create(ReadOnlySpan<VuetifySparklineItem> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySparklineItem : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _string;
    private readonly Number? _number;
    private readonly VuetifySparklineValueItem? _valueItem;

    private VuetifySparklineItem(string value)
    {
        _kind = 1;
        _string = value;
        _number = default;
        _valueItem = default;
    }

    private VuetifySparklineItem(Number value)
    {
        _kind = 2;
        _string = default;
        _number = value;
        _valueItem = default;
    }

    private VuetifySparklineItem(VuetifySparklineValueItem value)
    {
        _kind = 3;
        _string = default;
        _number = default;
        _valueItem = value;
    }

    public string? AsString => _kind == 1 ? _string : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public VuetifySparklineValueItem? AsValueItem => _kind == 3 ? _valueItem : default;

    public object? Value => _kind switch
    {
        1 => AsString,
        2 => AsNumber,
        3 => AsValueItem,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineItem From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineItem From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineItem From(VuetifySparklineValueItem value);

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

[ECMAScript]
[Description("@#")]
public sealed record VuetifySparklineValueItem : VueProps
{
    [Description("@#value")]
    public Number? Value { get; init; }
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySparklineSmoothValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly Number? _number;
    private readonly string? _string;

    private VuetifySparklineSmoothValue(bool value)
    {
        _kind = 1;
        _bool = value;
        _number = default;
        _string = default;
    }

    private VuetifySparklineSmoothValue(Number value)
    {
        _kind = 2;
        _bool = default;
        _number = value;
        _string = default;
    }

    private VuetifySparklineSmoothValue(string value)
    {
        _kind = 3;
        _bool = default;
        _number = default;
        _string = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public Number? AsNumber => _kind == 2 ? _number : default;

    public string? AsString => _kind == 3 ? _string : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsNumber,
        3 => AsString,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineSmoothValue From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineSmoothValue From(Number value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySparklineSmoothValue From(string value);

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
