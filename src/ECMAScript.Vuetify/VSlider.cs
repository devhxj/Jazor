using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑块组件的编写代理。
/// Vuetify slider authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSlider")]
public sealed class VSlider : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 滑块标签文本。
    /// Slider label text.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 组件颜色。
    /// Component color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 轨道颜色。
    /// Track color.
    /// </summary>
    [Parameter]
    public string? TrackColor { get; set; }

    /// <summary>
    /// 滑块手柄颜色。
    /// Thumb color.
    /// </summary>
    [Parameter]
    public string? ThumbColor { get; set; }

    /// <summary>
    /// 组件紧凑程度。
    /// Component density/compactness.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读模式。
    /// Puts the component in read-only mode.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否显示滑块手柄标签。
    /// Whether to show the thumb label.
    /// </summary>
    [Parameter]
    public VuetifyBooleanAlwaysValue? ThumbLabel { get; set; }

    /// <summary>
    /// 是否显示刻度线。
    /// Whether to show tick marks.
    /// </summary>
    [Parameter]
    public VuetifyBooleanAlwaysValue? ShowTicks { get; set; }

    /// <summary>
    /// 最小值。
    /// Minimum value.
    /// </summary>
    [Parameter]
    public Number? Min { get; set; }

    /// <summary>
    /// 最大值。
    /// Maximum value.
    /// </summary>
    [Parameter]
    public Number? Max { get; set; }

    /// <summary>
    /// 步进值。
    /// Step increment value.
    /// </summary>
    [Parameter]
    public Number? Step { get; set; }

    /// <summary>
    /// 是否使用严格步进模式。
    /// Uses strict step mode.
    /// </summary>
    [Parameter]
    public bool Strict { get; set; }

    /// <summary>
    /// 滑块方向。
    /// Slider direction.
    /// </summary>
    [Parameter]
    public VuetifySliderDirection? Direction { get; set; }

    /// <summary>
    /// 当前滑块值。
    /// Current slider value.
    /// </summary>
    [Parameter]
    public Number? ModelValue { get; set; }

    /// <summary>
    /// 滑块值变更回调。
    /// Callback when the slider value changes.
    /// </summary>
    [Parameter]
    public EventCallback<Number?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
