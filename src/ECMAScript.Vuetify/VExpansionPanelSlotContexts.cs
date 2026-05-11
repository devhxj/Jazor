namespace ECMAScript.Vuetify;

/// <summary>
/// Context exposed by Vuetify VExpansionPanel title slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VExpansionPanelTitleSlotContext
{
    [Description("@#collapseIcon")]
    public string? CollapseIcon { get; init; }

    [Description("@#disabled")]
    public bool Disabled { get; init; }

    [Description("@#expanded")]
    public bool Expanded { get; init; }

    [Description("@#expandIcon")]
    public string? ExpandIcon { get; init; }

    [Description("@#readonly")]
    public bool Readonly { get; init; }
}
