namespace ECMAScript.Vuetify;

/// <summary>
/// VRating 项插槽的上下文数据。
/// Context exposed by Vuetify VRating item slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VRatingItemSlotContext
{
    [Description("@#value")]
    public Number Value { get; init; }

    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#isFilled")]
    public bool IsFilled { get; init; }

    [Description("@#isHovered")]
    public bool IsHovered { get; init; }

    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

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
    [Description("@#value")]
    public Number Value { get; init; }

    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }
}
