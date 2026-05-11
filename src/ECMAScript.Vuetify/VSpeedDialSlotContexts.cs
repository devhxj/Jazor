namespace ECMAScript.Vuetify;

/// <summary>
/// Default slot context exposed by Vuetify VSpeedDial.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSpeedDialDefaultSlotContext
{
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}
