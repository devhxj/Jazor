namespace ECMAScript.Vuetify;

/// <summary>
/// VProgressCircular 默认插槽的上下文数据。
/// Slot context for the VProgressCircular default slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VProgressCircularDefaultSlotContext
{
    [Description("@#value")]
    public Number Value { get; init; }
}

/// <summary>
/// VProgressLinear 默认插槽的上下文数据。
/// Slot context for the VProgressLinear default slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VProgressLinearDefaultSlotContext
{
    [Description("@#value")]
    public Number Value { get; init; }

    [Description("@#buffer")]
    public Number Buffer { get; init; }
}
