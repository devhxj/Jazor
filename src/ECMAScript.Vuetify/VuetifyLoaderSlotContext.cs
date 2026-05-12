namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 加载器插槽暴露的上下文。
/// Context exposed by Vuetify loader slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyLoaderSlotContext
{
    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#isActive")]
    public bool IsActive { get; init; }
}
