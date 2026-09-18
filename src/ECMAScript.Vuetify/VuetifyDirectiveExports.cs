namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 指令的导出入口。
/// Export surface for Vuetify directives.
/// </summary>
[ECMAScript]
public static class VuetifyDirectives
{
    /// <summary>
    /// 检测目标元素外部的点击并调用处理函数的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/ClickOutside")]
    [ECMAScriptName("ClickOutside")]
    public extern static VuetifyDirective ClickOutside { get; }

    /// <summary>
    /// 观察目标元素与根区域交叉状态的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Intersect")]
    [ECMAScriptName("Intersect")]
    public extern static VuetifyDirective Intersect { get; }

    /// <summary>
    /// 观察目标 DOM 节点变化的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Mutate")]
    [ECMAScriptName("Mutate")]
    public extern static VuetifyDirective Mutate { get; }

    /// <summary>
    /// 监听尺寸变化并调用处理函数的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Resize")]
    [ECMAScriptName("Resize")]
    public extern static VuetifyDirective Resize { get; }

    /// <summary>
    /// 为点击或触摸交互添加水波纹效果的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Ripple")]
    [ECMAScriptName("Ripple")]
    public extern static VuetifyDirective Ripple { get; }

    /// <summary>
    /// 监听目标或滚动容器滚动事件的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Scroll")]
    [ECMAScriptName("Scroll")]
    public extern static VuetifyDirective Scroll { get; }

    /// <summary>
    /// 识别触摸和滑动方向的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Touch")]
    [ECMAScriptName("Touch")]
    public extern static VuetifyDirective Touch { get; }

    /// <summary>
    /// 为目标元素配置工具提示的指令。
    /// </summary>
    [ECMAScript("vuetify/directives/Tooltip")]
    [ECMAScriptName("Tooltip")]
    public extern static VuetifyDirective Tooltip { get; }
}

/// <summary>
/// Vuetify 指令的基类型。
/// Base type for Vuetify directives.
/// </summary>
[ECMAScript]
public abstract record VuetifyDirective : VueDirective
{
}
