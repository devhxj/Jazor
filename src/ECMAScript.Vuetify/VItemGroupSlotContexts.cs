namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VItemGroup 默认插槽上下文。
/// Default slot context exposed by Vuetify VItemGroup.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VItemGroupDefaultSlotContext
{
    [Description("@#isSelected")]
    public VuetifyGroupIsSelectedCallback? IsSelected { get; init; }

    [Description("@#select")]
    public VuetifyGroupSelectCallback? Select { get; init; }

    [Description("@#next")]
    public Action? Next { get; init; }

    [Description("@#prev")]
    public Action? Prev { get; init; }

    [Description("@#selected")]
    public VuetifyGroupModelValue[]? Selected { get; init; }
}

public delegate bool VuetifyGroupIsSelectedCallback(VuetifyGroupModelValue id);

public delegate void VuetifyGroupSelectCallback(VuetifyGroupModelValue id, bool value);
