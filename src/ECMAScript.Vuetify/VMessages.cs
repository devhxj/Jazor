using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息提示组件。
/// Vuetify messages component.
/// </summary>
[ECMAScript("vuetify/components/VMessages")]
public sealed class VMessages : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 消息是否处于活跃可见状态。
    /// Whether the messages are in an active visible state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 要显示的消息列表。
    /// List of messages to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 Messages="@value"，并保持声明的分支类型。字符串用 Messages="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 消息出现/消失时的过渡动画。
    /// Transition animation when messages appear or disappear.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTransitionValue?；值域为 bool | string | VueTransitionProps。变量用 Transition="@value"，并保持声明的分支类型。字符串用 Transition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsProps；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 单条消息插槽内容，提供消息槽位上下文。
    /// Individual message slot content, providing message slot context.
    /// </summary>
    [Parameter]
    [ECMAScriptName("message")]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }
}
