namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSpeedDial 默认插槽所暴露的插槽上下文。
/// Default slot context exposed by Vuetify VSpeedDial.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSpeedDialDefaultSlotContext
{
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}
