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
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    [Description("@#isOpen")]
    public bool IsOpen { get; init; }

    [Description("@#isSelected")]
    public bool IsSelected { get; init; }

    [Description("@#isIndeterminate")]
    public bool IsIndeterminate { get; init; }

    [Description("@#select")]
    public VListItemSelectCallback? Select { get; init; }
}

public delegate void VListItemSelectCallback(bool value);

/// <summary>
/// VListItem 标题插槽上下文。
/// Slot context for VListItem title slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VListItemTitleSlotContext
{
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
    [Description("@#subtitle")]
    public VuetifyTextValue? Subtitle { get; init; }
}
