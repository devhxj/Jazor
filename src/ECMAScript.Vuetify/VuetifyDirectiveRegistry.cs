namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 指令的注册表。
/// Registry of Vuetify directives.
/// </summary>
[ECMAScript]
[Description("@#VuetifyDirectiveRegistry")]
public sealed record VuetifyDirectiveRegistry : VueDirectiveRegistry
{
    /// <summary>
    /// 检测目标元素外部的点击并调用处理函数的指令。
    /// </summary>
    [Description("@#ClickOutside")]
    public VuetifyDirective? ClickOutside { get; init; }

    /// <summary>
    /// 观察目标元素与根区域交叉状态的指令。
    /// </summary>
    [Description("@#Intersect")]
    public VuetifyDirective? Intersect { get; init; }

    /// <summary>
    /// 观察目标 DOM 节点变化的指令。
    /// </summary>
    [Description("@#Mutate")]
    public VuetifyDirective? Mutate { get; init; }

    /// <summary>
    /// 监听尺寸变化并调用处理函数的指令。
    /// </summary>
    [Description("@#Resize")]
    public VuetifyDirective? Resize { get; init; }

    /// <summary>
    /// 为点击或触摸交互添加水波纹效果的指令。
    /// </summary>
    [Description("@#Ripple")]
    public VuetifyDirective? Ripple { get; init; }

    /// <summary>
    /// 监听目标或滚动容器滚动事件的指令。
    /// </summary>
    [Description("@#Scroll")]
    public VuetifyDirective? Scroll { get; init; }

    /// <summary>
    /// 为目标元素配置工具提示的指令。
    /// </summary>
    [Description("@#Tooltip")]
    public VuetifyDirective? Tooltip { get; init; }

    /// <summary>
    /// 识别触摸和滑动方向的指令。
    /// </summary>
    [Description("@#Touch")]
    public VuetifyDirective? Touch { get; init; }
}
