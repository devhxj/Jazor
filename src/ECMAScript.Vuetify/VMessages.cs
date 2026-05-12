using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息提示组件。
/// Vuetify messages component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VMessages")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(Message), Name = "message")]
public sealed class VMessages : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 消息是否处于活跃可见状态。
    /// Whether the messages are in an active visible state.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 要显示的消息列表。
    /// List of messages to display.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 消息出现/消失时的过渡动画。
    /// Transition animation when messages appear or disappear.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 单条消息插槽内容，提供消息槽位上下文。
    /// Individual message slot content, providing message slot context.
    /// </summary>
    [Parameter]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }
}
