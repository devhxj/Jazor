namespace ECMAScript.Vuetify;

/// <summary>
/// Load callback payload exposed by Vuetify VPullToRefresh.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VPullToRefreshLoadOptions
{
    [Description("@#done")]
    public Action? Done { get; init; }
}

/// <summary>
/// Pull-down panel slot context exposed by Vuetify VPullToRefresh.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VPullToRefreshPanelSlotContext
{
    [Description("@#canRefresh")]
    public bool CanRefresh { get; init; }

    [Description("@#goingUp")]
    public bool GoingUp { get; init; }

    [Description("@#refreshing")]
    public bool Refreshing { get; init; }
}
