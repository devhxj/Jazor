namespace ECMAScript.Vuetify;

/// <summary>
/// VSelect、VAutocomplete 和 VCombobox 的项目插槽所使用的插槽上下文。
/// Slot context used by VSelect, VAutocomplete, and VCombobox item slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSelectItemSlotContext
{
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    [Description("@#index")]
    public int Index { get; init; }

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
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    [Description("@#index")]
    public int Index { get; init; }

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
    [Description("@#item")]
    public VuetifyListItem? Item { get; init; }

    [Description("@#index")]
    public int Index { get; init; }
}
