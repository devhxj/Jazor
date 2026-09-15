namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VSnackbar 操作插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by the Vuetify VSnackbar actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarActionsSlotContext
{
    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#isActive")]
    public IVueRef<bool>? IsActive { get; init; }
}