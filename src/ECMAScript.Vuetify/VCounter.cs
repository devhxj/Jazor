using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 计数器组件创作代理。
/// Vuetify counter component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCounter", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VCounter : ComponentBase
{
    /// <summary>
    /// 是否显示计数器。
    /// Whether to show the counter.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// 是否禁用计数器。
    /// Whether to disable the counter.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 最大计数值。
    /// The maximum count value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 当前计数值。
    /// The current count value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Value { get; set; }

    /// <summary>
    /// 过渡动画效果。
    /// The transition animation effect.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认内容插槽。
    /// Default slot for counter content.
    /// </summary>
    [Parameter]
    public RenderFragment<VCounterDefaultSlotContext>? ChildContent { get; set; }
}
