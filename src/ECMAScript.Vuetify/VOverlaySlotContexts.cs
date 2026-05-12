namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 遮罩层系列组件共享的激活器插槽上下文。
/// Shared activator slot context exposed by Vuetify overlay-based components.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VOverlayActivatorContext
{
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#targetRef")]
    public VueValue? TargetRef { get; init; }
}
