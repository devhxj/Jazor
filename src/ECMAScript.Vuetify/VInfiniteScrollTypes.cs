namespace ECMAScript.Vuetify;

/// <summary>
/// 无限滚动加载方向枚举。
/// Infinite scroll loading side enumeration.
/// </summary>
[String]
public enum VuetifyInfiniteScrollSide
{
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 允许在滚动区域的起始和结束两侧加载；上游取值为 “both”。
    /// </summary>
    [Description("@#both")]
    Both
}

/// <summary>
/// 无限滚动触发模式枚举。
/// Infinite scroll trigger mode enumeration.
/// </summary>
[String]
public enum VuetifyInfiniteScrollMode
{
    /// <summary>
    /// 观察边界进入视口后自动加载；上游取值为 “intersect”。
    /// </summary>
    [Description("@#intersect")]
    Intersect,

    /// <summary>
    /// 通过显式操作触发加载；上游取值为 “manual”。
    /// </summary>
    [Description("@#manual")]
    Manual
}

/// <summary>
/// 无限滚动加载状态枚举。
/// Infinite scroll loading status enumeration.
/// </summary>
[String]
public enum VuetifyInfiniteScrollStatus
{
    /// <summary>
    /// 本次加载成功且可以继续加载；上游取值为 “ok”。
    /// </summary>
    [Description("@#ok")]
    Ok,

    /// <summary>
    /// 没有更多数据可加载；上游取值为 “empty”。
    /// </summary>
    [Description("@#empty")]
    Empty,

    /// <summary>
    /// 正在加载数据；上游取值为 “loading”。
    /// </summary>
    [Description("@#loading")]
    Loading,

    /// <summary>
    /// 错误状态样式；上游取值为 “error”。
    /// </summary>
    [Description("@#error")]
    Error
}

/// <summary>
/// 用于 VInfiniteScrollLoadOptions.Done 的回调签名。
/// 通知组件本次异步加载已结束；有状态参数时需传入成功、空数据或错误状态。
/// </summary>
public delegate void VInfiniteScrollDoneCallback(VuetifyInfiniteScrollStatus status);

/// <summary>
/// Vuetify VInfiniteScroll 加载事件发出的载荷。
/// Payload emitted by Vuetify VInfiniteScroll load events.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VInfiniteScrollLoadOptions
{
    /// <summary>
    /// 本次加载触发的滚动方向或位置。
    /// </summary>
    [Description("@#side")]
    public VuetifyInfiniteScrollSide Side { get; init; }

    /// <summary>
    /// 通知组件本次异步加载已结束；有状态参数时需传入成功、空数据或错误状态。
    /// </summary>
    [Description("@#done")]
    public VInfiniteScrollDoneCallback? Done { get; init; }
}

/// <summary>
/// VInfiniteScroll 状态插槽使用的作用域插槽上下文。
/// Scoped slot context used by VInfiniteScroll status slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VInfiniteScrollSlotContext
{
    /// <summary>
    /// 本次加载触发的滚动方向或位置。
    /// </summary>
    [Description("@#side")]
    public VuetifyInfiniteScrollSide Side { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}