using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 计数器组件创作代理。
/// Vuetify counter component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VCounter")]
public sealed class VCounter : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否显示计数器。
    /// Whether to show the counter.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 是否禁用计数器。
    /// Whether to disable the counter.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 最大计数值。
    /// The maximum count value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 当前计数值。
    /// The current count value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Value="@(32)"；变量用 Value="@value"，无需 double 后缀。字符串用 Value="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("value")]
    public VueStringNumberValue? Value { get; set; }

    /// <summary>
    /// 过渡动画效果。
    /// The transition animation effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTransitionValue?；值域为 bool | string | VueTransitionProps。变量用 Transition="@value"，并保持声明的分支类型。字符串用 Transition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsProps；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认内容插槽。
    /// Default slot for counter content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VCounterDefaultSlotContext>? ChildContent { get; set; }
}
