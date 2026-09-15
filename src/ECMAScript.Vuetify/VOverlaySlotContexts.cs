namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 遮罩层系列组件共享的激活器插槽上下文。
/// Shared activator slot context exposed by Vuetify overlay-based components.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VOverlayActivatorContext
{
    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。
    /// </summary>
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }

    /// <summary>
    /// 定位浮层所用的目标元素引用；应将其绑定到实际目标元素。
    /// </summary>
    [Description("@#targetRef")]
    public VueValue? TargetRef { get; init; }
}