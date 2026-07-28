using Microsoft.AspNetCore.Components;

namespace Jazor.Admin;

[ECMAScript]
[Description("@#")]
public sealed record AdminLayoutState : VueProps
{
    [Description("@#collapsed")]
    public bool? Collapsed { get; init; }

    [Description("@#mode")]
    public AdminLayoutMode? Mode { get; init; }

    [Description("@#selectedKey")]
    public string? SelectedKey { get; init; }

    [Description("@#expandedKeys")]
    public string[]? ExpandedKeys { get; init; }
}
