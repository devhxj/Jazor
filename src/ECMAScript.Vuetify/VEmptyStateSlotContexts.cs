namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 对齐方式枚举。
/// Vuetify justify alignment enumeration.
/// </summary>
[String]
public enum VuetifyJustify
{
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End
}

/// <summary>
/// Vuetify VEmptyState 操作插槽暴露的属性对象。
/// Props object exposed by the Vuetify VEmptyState actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VEmptyStateActionsProps
{
    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action<EventRef>? OnClick { get; init; }
}

/// <summary>
/// Vuetify VEmptyState 作用域操作插槽上下文。
/// Scoped actions slot context exposed by Vuetify VEmptyState.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VEmptyStateActionsSlotContext
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VEmptyStateActionsProps? Props { get; init; }
}
