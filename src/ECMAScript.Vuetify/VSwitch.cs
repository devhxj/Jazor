using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 开关组件的编写代理。
/// Vuetify switch authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSwitch")]
public sealed class VSwitch : VSelectionControlComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否使用缩进样式。
    /// Whether to use inset style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inset")]
    public bool Inset { get; set; }

    /// <summary>
    /// 加载状态。
    /// Loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loading")]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 是否移除阴影效果。
    /// Removes box-shadow.
    /// </summary>
    [Parameter]
    [ECMAScriptName("flat")]
    public bool Flat { get; set; }

    /// <summary>
    /// 加载插槽内容。
    /// Loader slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loader")]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }

    /// <summary>
    /// 滑块插槽内容。
    /// Thumb slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thumb")]
    public RenderFragment<VSwitchSlotContext>? Thumb { get; set; }

    /// <summary>
    /// 开启轨道插槽内容。
    /// Track-true slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("track-true")]
    public RenderFragment<VSwitchSlotContext>? TrackTrue { get; set; }

    /// <summary>
    /// 关闭轨道插槽内容。
    /// Track-false slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("track-false")]
    public RenderFragment<VSwitchSlotContext>? TrackFalse { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
