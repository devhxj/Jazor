namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VHover 默认插槽上下文。
/// Default slot context exposed by Vuetify VHover.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VHoverDefaultSlotContext
{
    /// <summary>
    /// 指针是否正悬停在目标区域。
    /// </summary>
    [Description("@#isHovering")]
    public bool IsHovering { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}