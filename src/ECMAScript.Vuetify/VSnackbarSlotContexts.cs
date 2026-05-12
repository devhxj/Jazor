namespace ECMAScript.Vuetify;

/// <summary>
/// Scoped slot context exposed by the Vuetify VSnackbar actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarActionsSlotContext
{
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}
