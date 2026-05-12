namespace ECMAScript.Vuetify;

/// <summary>
/// VPullToRefresh 加载回调的负载数据。
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
/// VPullToRefresh 下拉面板插槽的上下文数据。
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
