namespace ECMAScript.Vuetify;

/// <summary>
/// VPullToRefresh 加载回调的负载数据。
/// Load callback payload exposed by Vuetify VPullToRefresh.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VPullToRefreshLoadOptions
{
    /// <summary>
    /// 通知组件本次异步加载已结束；有状态参数时需传入成功、空数据或错误状态。
    /// </summary>
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
    /// <summary>
    /// 下拉距离是否已达到可触发刷新的阈值。
    /// </summary>
    [Description("@#canRefresh")]
    public bool CanRefresh { get; init; }

    /// <summary>
    /// 当前手势是否朝向收回下拉面板的方向。
    /// </summary>
    [Description("@#goingUp")]
    public bool GoingUp { get; init; }

    /// <summary>
    /// 是否正在执行刷新操作。
    /// </summary>
    [Description("@#refreshing")]
    public bool Refreshing { get; init; }
}