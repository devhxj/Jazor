namespace ECMAScript.Vuetify;

/// <summary>
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

[ECMAScript]
[Description("@#")]
public sealed record VListItemTitleSlotContext
{
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record VListItemSubtitleSlotContext
{
    [Description("@#subtitle")]
    public VuetifyTextValue? Subtitle { get; init; }
}
