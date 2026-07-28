namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 轮播垂直分隔线位置枚举。
/// Vuetify carousel vertical delimiter position enum.
/// </summary>
[String]
public enum VuetifyCarouselVerticalDelimiterPosition
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

/// <summary>
/// Vuetify 轮播垂直分隔线值，匹配 <c>boolean | "left" | "right"</c>。
/// Vuetify carousel vertical-delimiters value, matching <c>boolean | "left" | "right"</c>.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCarouselVerticalDelimiters : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyCarouselVerticalDelimiterPosition? _position;

    public VuetifyCarouselVerticalDelimiters(bool value)
    {
        _kind = 1;
        _bool = value;
        _position = default;
    }

    public VuetifyCarouselVerticalDelimiters(VuetifyCarouselVerticalDelimiterPosition value)
    {
        _kind = 2;
        _bool = default;
        _position = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyCarouselVerticalDelimiterPosition? AsPosition => _kind == 2 ? _position : default;

    public object? Value => _kind switch
    {
        1 => AsBool,
        2 => AsPosition,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCarouselVerticalDelimiters From(bool value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCarouselVerticalDelimiters From(VuetifyCarouselVerticalDelimiterPosition value);

    public static implicit operator VuetifyCarouselVerticalDelimiters(bool value)
        => new(value);

    public static implicit operator VuetifyCarouselVerticalDelimiters(VuetifyCarouselVerticalDelimiterPosition value)
        => new(value);
}

/// <summary>
/// Vuetify VCarousel 公开的项插槽上下文，用于自定义分隔符渲染。
/// Item slot context exposed by Vuetify VCarousel for custom delimiter rendering.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCarouselItemSlotContext
{
    [Description("@#props")]
    public VCarouselItemSlotProps? Props { get; init; }

    [Description("@#item")]
    public VuetifyWindowGroupItem? Item { get; init; }
}

/// <summary>
/// 提供给 Vuetify VCarousel 项插槽的属性对象。
/// Props object provided to Vuetify VCarousel item slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCarouselItemSlotProps : VueProps
{
    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#aria-label")]
    public string? AriaLabel { get; init; }

    [Description("@#class")]
    public VueClassValue? Class { get; init; }

    [Description("@#onClick")]
    public Action? OnClick { get; init; }
}
