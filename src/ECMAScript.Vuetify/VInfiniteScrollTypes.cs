namespace ECMAScript.Vuetify;

/// <summary>
/// 无限滚动加载方向枚举。
/// Infinite scroll loading side enumeration.
/// </summary>
[String]
public enum VuetifyInfiniteScrollSide
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

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
    [Description("@#intersect")]
    Intersect,

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
    [Description("@#ok")]
    Ok,

    [Description("@#empty")]
    Empty,

    [Description("@#loading")]
    Loading,

    [Description("@#error")]
    Error
}

public delegate void VInfiniteScrollDoneCallback(VuetifyInfiniteScrollStatus status);

/// <summary>
/// Vuetify VInfiniteScroll 加载事件发出的载荷。
/// Payload emitted by Vuetify VInfiniteScroll load events.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VInfiniteScrollLoadOptions
{
    [Description("@#side")]
    public VuetifyInfiniteScrollSide Side { get; init; }

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
    [Description("@#side")]
    public VuetifyInfiniteScrollSide Side { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }
}
