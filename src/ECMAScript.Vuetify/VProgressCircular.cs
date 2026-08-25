using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 圆形进度指示器组件。
/// Vuetify circular progress indicator component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VProgressCircular")]
public sealed class VProgressCircular : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆形进度指示器的尺寸。
    /// The size of the circular progress indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 应用于根元素的 CSS 类。
    /// CSS classes applied to the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于根元素的内联样式。
    /// Inline styles applied to the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 进度指示器的颜色。
    /// The color of the progress indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 进度指示器的背景颜色。
    /// The background color of the progress indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示为不确定状态的动画。
    /// Whether to display an indeterminate animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("indeterminate")]
    public VuetifyProgressCircularIndeterminateValue? Indeterminate { get; set; }

    /// <summary>
    /// 当前进度百分比数值。
    /// The current progress percentage value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueStringNumberValue? ModelValue { get; set; }

    /// <summary>
    /// 进度指示器的旋转角度。
    /// The rotation angle of the progress indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rotate")]
    public VueStringNumberValue? Rotate { get; set; }

    /// <summary>
    /// 进度弧线的宽度。
    /// The stroke width of the progress arc.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VProgressCircularDefaultSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
