namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VCounter 公开的默认插槽上下文。
/// Default slot context exposed by Vuetify VCounter.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCounterDefaultSlotContext
{
    /// <summary>
    /// 已格式化的计数器显示文本。
    /// </summary>
    [Description("@#counter")]
    public string? Counter { get; init; }

    /// <summary>
    /// 计数器的最大允许值，用于显示上限提示。
    /// </summary>
    [Description("@#max")]
    public VueStringNumberValue? Max { get; init; }

    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public VueStringNumberValue? Value { get; init; }
}