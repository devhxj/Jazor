using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben;

[ECMAScript]
[Description("@#")]
public sealed record VbenAdminLayoutState : VueProps
{
    [Description("@#collapsed")]
    public bool? Collapsed { get; init; }

    [Description("@#mode")]
    public VbenLayoutMode? Mode { get; init; }

    [Description("@#selectedKey")]
    public string? SelectedKey { get; init; }

    [Description("@#expandedKeys")]
    public string[]? ExpandedKeys { get; init; }
}
