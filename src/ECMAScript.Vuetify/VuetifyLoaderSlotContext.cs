namespace ECMAScript.Vuetify;

/// <summary>
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
