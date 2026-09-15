namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSpeedDial 默认插槽所暴露的插槽上下文。
/// Default slot context exposed by Vuetify VSpeedDial.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSpeedDialDefaultSlotContext
{
    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}