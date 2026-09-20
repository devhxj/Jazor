#nullable enable

namespace ECMAScript.ElementPlus;

/// <summary>
/// Export surface for Element Plus directives.
/// </summary>
[ECMAScript("element-plus/es/index.mjs")]
public static class ElDirectives
{
    /// <summary>
    /// 滚动接近边界时请求加载更多数据的 v-infinite-scroll 指令。
    /// </summary>
    [ECMAScriptName("ElInfiniteScroll")]
    public extern static ElDirective InfiniteScroll { get; }

    /// <summary>
    /// 为目标元素显示加载遮罩的 v-loading 指令。
    /// </summary>
    [ECMAScriptName("ElLoadingDirective")]
    public extern static VueDirective<ElDirectiveValue> Loading { get; }

}
