using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 范围滑块组件。
/// Vuetify range slider component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VRangeSlider")]
public sealed class VRangeSlider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 范围滑块的标签文本。
    /// The label text of the range slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 滑块轨道的填充颜色。
    /// The fill color of the slider track.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 滑块轨道的背景颜色。
    /// The background color of the slider track.
    /// </summary>
    [Parameter]
    [ECMAScriptName("trackColor")]
    public string? TrackColor { get; set; }

    /// <summary>
    /// 滑块滑块的颜色。
    /// The color of the slider thumb.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thumbColor")]
    public string? ThumbColor { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用滑块。
    /// Whether the slider is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the slider is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否在滑块上方显示数值标签。
    /// Whether to show a value label above the thumb.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thumbLabel")]
    public VuetifyBooleanAlwaysValue? ThumbLabel { get; set; }

    /// <summary>
    /// 是否显示刻度标记。
    /// Whether to show tick marks on the track.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showTicks")]
    public VuetifyBooleanAlwaysValue? ShowTicks { get; set; }

    /// <summary>
    /// 滑块的最小值。
    /// The minimum value of the slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("min")]
    public Number? Min { get; set; }

    /// <summary>
    /// 滑块的最大值。
    /// The maximum value of the slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public Number? Max { get; set; }

    /// <summary>
    /// 滑块的步进值。
    /// The step increment of the slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("step")]
    public Number? Step { get; set; }

    /// <summary>
    /// 是否启用严格模式，禁止两个滑块重叠。
    /// Whether to enable strict mode, preventing thumb overlap.
    /// </summary>
    [Parameter]
    [ECMAScriptName("strict")]
    public bool Strict { get; set; }

    /// <summary>
    /// 滑块的方向。
    /// The direction of the slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifySliderDirection? Direction { get; set; }

    /// <summary>
    /// 当前范围滑块的值。
    /// The current value of the range slider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyRangeSliderModelValue? ModelValue { get; set; }

    /// <summary>
    /// 范围值变更时触发的回调。
    /// Callback invoked when the range value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyRangeSliderModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
