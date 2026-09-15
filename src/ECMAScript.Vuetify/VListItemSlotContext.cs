namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VListItem 前置、后置、标题、副标题和默认插槽的插槽上下文。
/// Slot context exposed by Vuetify VListItem prepend, append, title,
/// subtitle, and default slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VListItemSlotContext
{
    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。
    /// </summary>
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    /// <summary>
    /// 当前节点或分组是否已展开。
    /// </summary>
    [Description("@#isOpen")]
    public bool IsOpen { get; init; }

    /// <summary>
    /// 当前条目是否已选中。
    /// </summary>
    [Description("@#isSelected")]
    public bool IsSelected { get; init; }

    /// <summary>
    /// 当前节点是否处于部分子项选中的中间状态。
    /// </summary>
    [Description("@#isIndeterminate")]
    public bool IsIndeterminate { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VListItemSelectCallback? Select { get; init; }
}

/// <summary>
/// 用于 VListItemSlotContext.Select、VTreeviewNodeSlotContext.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
public delegate void VListItemSelectCallback(bool value);

/// <summary>
/// VListItem 标题插槽上下文。
/// Slot context for VListItem title slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VListItemTitleSlotContext
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }
}

/// <summary>
/// VListItem 副标题插槽上下文。
/// Slot context for VListItem subtitle slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VListItemSubtitleSlotContext
{
    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VuetifyTextValue? Subtitle { get; init; }
}