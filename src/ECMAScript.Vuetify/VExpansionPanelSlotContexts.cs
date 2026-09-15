namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VExpansionPanel 标题插槽上下文。
/// Context exposed by Vuetify VExpansionPanel title slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VExpansionPanelTitleSlotContext
{
    /// <summary>
    /// 用于表示可收起状态的图标。
    /// </summary>
    [Description("@#collapseIcon")]
    public string? CollapseIcon { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。
    /// </summary>
    [Description("@#disabled")]
    public bool Disabled { get; init; }

    /// <summary>
    /// 当前面板是否展开。
    /// </summary>
    [Description("@#expanded")]
    public bool Expanded { get; init; }

    /// <summary>
    /// 用于表示可展开状态的图标。
    /// </summary>
    [Description("@#expandIcon")]
    public string? ExpandIcon { get; init; }

    /// <summary>
    /// 当前面板是否只读。
    /// </summary>
    [Description("@#readonly")]
    public bool Readonly { get; init; }
}