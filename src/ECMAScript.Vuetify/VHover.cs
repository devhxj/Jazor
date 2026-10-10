using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 悬停组件，用于检测和响应鼠标悬停状态。
/// Vuetify hover component for detecting and responding to mouse hover state.
/// </summary>
[ECMAScript("vuetify/components/VHover")]
public sealed class VHover : ComponentBase, IVuetifyComponent
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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OpenDelay="@(32)"；变量用 OpenDelay="@value"，无需 double 后缀。字符串用 OpenDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭悬停状态的延迟时间（毫秒）。
    /// Delay in milliseconds before deactivating the hover state.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 CloseDelay="@(32)"；变量用 CloseDelay="@value"，无需 double 后缀。字符串用 CloseDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
