#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Registry of Element Plus directives.
/// </summary>
[ECMAScript]
[Description("@#ElDirectiveRegistry")]
public sealed record ElDirectiveRegistry : VueDirectiveRegistry
{
    /// <summary>
    /// 滚动接近边界时请求加载更多数据的 v-infinite-scroll 指令。
    /// </summary>
    [Description("@#InfiniteScroll")]
    public ElDirective? InfiniteScroll { get; init; }

    /// <summary>
    /// 为目标元素显示加载遮罩的 v-loading 指令。
    /// </summary>
    [Description("@#Loading")]
    public VueDirective<ElDirectiveValue>? Loading { get; init; }

}
