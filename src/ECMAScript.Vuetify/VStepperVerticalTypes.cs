namespace ECMAScript.Vuetify;

[String]
public enum VuetifyExpansionPanelVariant
{
    [Description("@#default")]
    Default,

    [Description("@#accordion")]
    Accordion,

    [Description("@#inset")]
    Inset,

    [Description("@#popout")]
    Popout
}

/// <summary>
/// Default slot context exposed by Vuetify labs VStepperVertical.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperVerticalSlotContext
{
    [Description("@#prev")]
    public Action? Prev { get; init; }

    [Description("@#next")]
    public Action? Next { get; init; }

    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }
}

/// <summary>
/// Item/icon/title/subtitle and dynamic item slot context exposed by Vuetify labs VStepperVertical.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperVerticalItemSlotContext
{
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    [Description("@#hasError")]
    public bool HasError { get; init; }

    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }
}

/// <summary>
/// Action slot context exposed by Vuetify labs VStepperVertical actions, prev, and next slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VStepperVerticalActionSlotContext
{
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    [Description("@#hasError")]
    public bool HasError { get; init; }

    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }

    [Description("@#prev")]
    public Action? Prev { get; init; }

    [Description("@#next")]
    public Action? Next { get; init; }
}
