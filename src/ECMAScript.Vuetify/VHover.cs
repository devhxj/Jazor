using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 悬停组件，用于检测和响应鼠标悬停状态。
/// Vuetify hover component for detecting and responding to mouse hover state.
/// </summary>
[VueLibraryComponent("vuetify/components", "VHover")]
public sealed class VHover : ComponentBase
{
    /// <summary>
    /// 是否禁用悬停检测。
    /// Whether to disable hover detection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 悬停状态的模型值。
    /// The hover state model value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 悬停状态变化时的回调。
    /// Callback when the hover state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 打开悬停状态的延迟时间（毫秒）。
    /// Delay in milliseconds before activating the hover state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭悬停状态的延迟时间（毫秒）。
    /// Delay in milliseconds before deactivating the hover state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 悬停组件的默认插槽内容。
    /// Default slot content of the hover component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VHoverDefaultSlotContext>? ChildContent { get; set; }
}
