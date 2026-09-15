namespace ECMAScript.Vuetify;

/// <summary>
/// VSelect、VAutocomplete 和 VCombobox 的项目插槽所使用的插槽上下文。
/// Slot context used by VSelect, VAutocomplete, and VCombobox item slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectItemSlotContext
{
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}

/// <summary>
/// 将选中项渲染为芯片时使用的插槽上下文。
/// Slot context used when rendering a selected item as a chip.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectChipSlotContext
{
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}

/// <summary>
/// 在未使用芯片的情况下渲染选中项时使用的插槽上下文。
/// Slot context used when rendering a selected item without chips.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectSelectionSlotContext
{
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }
}