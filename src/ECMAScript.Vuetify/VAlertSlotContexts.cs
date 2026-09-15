namespace ECMAScript.Vuetify;

/// <summary>
/// VAlert 关闭插槽的上下文对象。
/// Close slot context exposed by Vuetify VAlert.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VAlertCloseSlotContext
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }
}