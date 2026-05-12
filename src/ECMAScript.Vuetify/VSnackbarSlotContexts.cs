namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSnackbar 操作插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by the Vuetify VSnackbar actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarActionsSlotContext
{
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}
