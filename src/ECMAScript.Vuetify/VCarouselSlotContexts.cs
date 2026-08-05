namespace ECMAScript.Vuetify;

// Defines VCarousel-specific slot values and scoped-slot context records.
// 定义 VCarousel 专用的插槽值域和作用域插槽上下文；可擦除值域使用原生 union。

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
[Description("@#")]
public readonly union VuetifyCarouselVerticalDelimiters(bool, VuetifyCarouselVerticalDelimiterPosition)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyCarouselVerticalDelimiterPosition? AsPosition
        => Value is VuetifyCarouselVerticalDelimiterPosition value
            ? value
            : default(VuetifyCarouselVerticalDelimiterPosition?);

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
