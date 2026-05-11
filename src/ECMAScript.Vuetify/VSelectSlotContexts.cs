namespace ECMAScript.Vuetify;

/// <summary>
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
