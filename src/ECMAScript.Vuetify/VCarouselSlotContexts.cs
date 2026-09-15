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
    /// <summary>
    /// 左侧；上游取值为 “left”。
    /// </summary>
    [Description("@#left")]
    Left,

    /// <summary>
    /// 右侧；上游取值为 “right”。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VuetifyCarouselVerticalDelimiterPosition 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCarouselVerticalDelimiterPosition? AsPosition
        => Value is VuetifyCarouselVerticalDelimiterPosition value
            ? value
            : default(VuetifyCarouselVerticalDelimiterPosition?);

    /// <summary>
    /// 将 bool 值转换为 VuetifyCarouselVerticalDelimiters，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCarouselVerticalDelimiters(bool value)
        => new(value);

    /// <summary>
    /// 将 VuetifyCarouselVerticalDelimiterPosition 值转换为 VuetifyCarouselVerticalDelimiters，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VCarouselItemSlotProps? Props { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
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
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 用于辅助技术描述当前操作的 aria-label 文本。
    /// </summary>
    [Description("@#aria-label")]
    public string? AriaLabel { get; init; }

    /// <summary>
    /// 需要转发给渲染目标的 CSS 类名。
    /// </summary>
    [Description("@#class")]
    public VueClassValue? Class { get; init; }

    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action? OnClick { get; init; }
}