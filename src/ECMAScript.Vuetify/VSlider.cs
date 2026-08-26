using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑块组件的编写代理。
/// Vuetify slider authoring proxy.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VSlider")]
public sealed class VSlider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 滑块标签文本。
    /// Slider label text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 组件颜色。
    /// Component color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 轨道颜色。
    /// Track color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("trackColor")]
    public string? TrackColor { get; set; }

    /// <summary>
    /// 滑块手柄颜色。
    /// Thumb color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thumbColor")]
    public string? ThumbColor { get; set; }

    /// <summary>
    /// 组件紧凑程度。
    /// Component density/compactness.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读模式。
    /// Puts the component in read-only mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否显示滑块手柄标签。
    /// Whether to show the thumb label.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thumbLabel")]
    public VuetifyBooleanAlwaysValue? ThumbLabel { get; set; }

    /// <summary>
    /// 是否显示刻度线。
    /// Whether to show tick marks.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showTicks")]
    public VuetifyBooleanAlwaysValue? ShowTicks { get; set; }

    /// <summary>
    /// 最小值。
    /// Minimum value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("min")]
    public Number? Min { get; set; }

    /// <summary>
    /// 最大值。
    /// Maximum value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public Number? Max { get; set; }

    /// <summary>
    /// 步进值。
    /// Step increment value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("step")]
    public Number? Step { get; set; }

    /// <summary>
    /// 是否使用严格步进模式。
    /// Uses strict step mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("strict")]
    public bool Strict { get; set; }

    /// <summary>
    /// 滑块方向。
    /// Slider direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifySliderDirection? Direction { get; set; }

    /// <summary>
    /// 当前滑块值。
    /// Current slider value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public Number? ModelValue { get; set; }

    /// <summary>
    /// 滑块值变更回调。
    /// Callback when the slider value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<Number?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
