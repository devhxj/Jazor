namespace ECMAScript.Vuetify;

/// <summary>
/// 展开面板变体枚举。
/// Expansion panel variant enum.
/// </summary>
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
/// Vuetify 实验室 VStepperVertical 默认插槽所暴露的插槽上下文。
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
/// Vuetify 实验室 VStepperVertical 的项目/图标/标题/副标题和动态项目插槽上下文。
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
/// Vuetify 实验室 VStepperVertical 操作、上一步和下一步插槽所暴露的操作插槽上下文。
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
