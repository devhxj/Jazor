namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VVirtualScroll 暴露的作用域默认插槽上下文。
/// Scoped default slot context exposed by Vuetify VVirtualScroll.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VVirtualScrollSlotContext
{
    [Description("@#item")]
    public VueValue? Item { get; init; }

    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#itemRef")]
    public IVueRef<HTMLElement?>? ItemRef { get; init; }
}
