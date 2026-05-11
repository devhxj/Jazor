namespace ECMAScript.Vuetify;

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

[String]
public enum VuetifyInfiniteScrollMode
{
    [Description("@#intersect")]
    Intersect,

    [Description("@#manual")]
    Manual
}

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
