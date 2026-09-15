namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 加载器插槽暴露的上下文。
/// Context exposed by Vuetify loader slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyLoaderSlotContext
{
    /// <summary>
    /// 当前项的主题颜色名或 CSS 颜色值。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。
    /// </summary>
    [Description("@#isActive")]
    public bool IsActive { get; init; }
}