namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VVirtualScroll 暴露的作用域默认插槽上下文。
/// Scoped default slot context exposed by Vuetify VVirtualScroll.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VVirtualScrollSlotContext
{
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VueValue? Item { get; init; }

    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 绑定到当前虚拟滚动条目元素的引用，用于测量条目尺寸。 通过引用的 Value 读取最新状态。
    /// </summary>
    [Description("@#itemRef")]
    public IVueRef<HTMLElement?>? ItemRef { get; init; }
}