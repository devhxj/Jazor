namespace ECMAScript.Vuetify;

/// <summary>
/// VProgressCircular 默认插槽的上下文数据。
/// Slot context for the VProgressCircular default slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VProgressCircularDefaultSlotContext
{
    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
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
    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public Number Value { get; init; }

    /// <summary>
    /// 缓冲进度值，用于线性进度条的缓冲区域。
    /// </summary>
    [Description("@#buffer")]
    public Number Buffer { get; init; }
}