namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VHover 默认插槽上下文。
/// Default slot context exposed by Vuetify VHover.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VHoverDefaultSlotContext
{
    [Description("@#isHovering")]
    public bool IsHovering { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }
}
