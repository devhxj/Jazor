namespace ECMAScript.Vuetify;

/// <summary>
/// 展开面板变体枚举。
/// Expansion panel variant enum.
/// </summary>
[String]
public enum VuetifyExpansionPanelVariant
{
    /// <summary>
    /// 默认配置；上游取值为 “default”。
    /// </summary>
    [Description("@#default")]
    Default,

    /// <summary>
    /// 相邻面板紧贴的手风琴样式；上游取值为 “accordion”。
    /// </summary>
    [Description("@#accordion")]
    Accordion,

    /// <summary>
    /// 展开面板内缩的样式；上游取值为 “inset”。
    /// </summary>
    [Description("@#inset")]
    Inset,

    /// <summary>
    /// 展开面板向外突出的样式；上游取值为 “popout”。
    /// </summary>
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
    /// <summary>
    /// 移动到组中的上一个可选项。
    /// </summary>
    [Description("@#prev")]
    public Action? Prev { get; init; }

    /// <summary>
    /// 移动到组中的下一个可选项。
    /// </summary>
    [Description("@#next")]
    public Action? Next { get; init; }

    /// <summary>
    /// 当前步骤关联的模型值，用于识别导航目标。
    /// </summary>
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
    /// <summary>
    /// 是否允许用户进入并编辑此步骤。
    /// </summary>
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    /// <summary>
    /// 当前步骤是否处于错误状态。
    /// </summary>
    [Description("@#hasError")]
    public bool HasError { get; init; }

    /// <summary>
    /// 当前步骤是否已完成。
    /// </summary>
    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    /// <summary>
    /// 当前步骤关联的模型值，用于识别导航目标。
    /// </summary>
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
    /// <summary>
    /// 是否允许用户进入并编辑此步骤。
    /// </summary>
    [Description("@#canEdit")]
    public bool CanEdit { get; init; }

    /// <summary>
    /// 当前步骤是否处于错误状态。
    /// </summary>
    [Description("@#hasError")]
    public bool HasError { get; init; }

    /// <summary>
    /// 当前步骤是否已完成。
    /// </summary>
    [Description("@#hasCompleted")]
    public bool HasCompleted { get; init; }

    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VueStringNumberValue? Title { get; init; }

    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VueStringNumberValue? Subtitle { get; init; }

    /// <summary>
    /// 当前步骤关联的模型值，用于识别导航目标。
    /// </summary>
    [Description("@#step")]
    public VuetifyGroupModelValue? Step { get; init; }

    /// <summary>
    /// 移动到组中的上一个可选项。
    /// </summary>
    [Description("@#prev")]
    public Action? Prev { get; init; }

    /// <summary>
    /// 移动到组中的下一个可选项。
    /// </summary>
    [Description("@#next")]
    public Action? Next { get; init; }
}