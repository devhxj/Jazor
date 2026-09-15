namespace ECMAScript.Vuetify;

/// <summary>
/// VRating 项插槽的上下文数据。
/// Context exposed by Vuetify VRating item slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VRatingItemSlotContext
{
    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public Number Value { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 当前评分项是否应显示为已填充状态。
    /// </summary>
    [Description("@#isFilled")]
    public bool IsFilled { get; init; }

    /// <summary>
    /// 当前评分项是否处于指针预览状态。
    /// </summary>
    [Description("@#isHovered")]
    public bool IsHovered { get; init; }

    /// <summary>
    /// 当前状态所用的图标，支持 Vuetify 图标别名或相应图标值。
    /// </summary>
    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}

/// <summary>
/// VRating 项标签插槽的上下文数据。
/// Context exposed by Vuetify VRating item-label slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VRatingItemLabelSlotContext
{
    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public Number Value { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 供当前控件或数据项显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}