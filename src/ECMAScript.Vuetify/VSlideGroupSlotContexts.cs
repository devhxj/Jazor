namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSlideGroup 默认、上一步和下一步插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by Vuetify VSlideGroup default, prev, and next slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSlideGroupSlotContext
{
    [Description("@#next")]
    public Action? Next { get; init; }

    [Description("@#prev")]
    public Action? Prev { get; init; }

    [Description("@#select")]
    public VuetifySlideGroupSelectCallback? Select { get; init; }

    [Description("@#isSelected")]
    public VuetifySlideGroupIsSelectedCallback? IsSelected { get; init; }
}

public delegate void VuetifySlideGroupSelectCallback(string id, bool value);

public delegate bool VuetifySlideGroupIsSelectedCallback(string id);
