namespace ECMAScript.Vuetify;

[String]
public enum VuetifyCarouselVerticalDelimiterPosition
{
    [Description("@#left")]
    Left,

    [Description("@#right")]
    Right
}

/// <summary>
/// Vuetify carousel vertical-delimiters value, matching <c>boolean | "left" | "right"</c>.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyCarouselVerticalDelimiters
{
    private readonly byte _kind;
    private readonly bool? _bool;
    private readonly VuetifyCarouselVerticalDelimiterPosition? _position;

    private VuetifyCarouselVerticalDelimiters(bool value)
    {
        _kind = 1;
        _bool = value;
        _position = default;
    }

    private VuetifyCarouselVerticalDelimiters(VuetifyCarouselVerticalDelimiterPosition value)
    {
        _kind = 2;
        _bool = default;
        _position = value;
    }

    public bool? AsBool => _kind == 1 ? _bool : default;

    public VuetifyCarouselVerticalDelimiterPosition? AsPosition => _kind == 2 ? _position : default;

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
