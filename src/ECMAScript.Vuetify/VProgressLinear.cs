using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 线性进度条组件。
/// Vuetify linear progress bar component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VProgressLinear")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
public sealed class VProgressLinear : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角大小。
    /// The border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角，使边角为直角。
    /// Whether to remove border radius for sharp corners.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 进度条的方位。
    /// The location of the progress bar.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 应用于根元素的 CSS 类。
    /// CSS classes applied to the root element.
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于根元素的内联样式。
    /// Inline styles applied to the root element.
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    public bool Absolute { get; set; }

    /// <summary>
    /// 是否激活显示进度条。
    /// Whether the progress bar is active and visible.
    /// </summary>
    [Parameter]
    public bool Active { get; set; } = true;

    /// <summary>
    /// 进度条的颜色。
    /// The color of the progress bar.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 进度条的背景颜色。
    /// The background color of the progress bar.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 背景透明度。
    /// The background opacity.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? BgOpacity { get; set; }

    /// <summary>
    /// 缓冲区的进度值。
    /// The buffer progress value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? BufferValue { get; set; }

    /// <summary>
    /// 缓冲区的颜色。
    /// The color of the buffer track.
    /// </summary>
    [Parameter]
    public string? BufferColor { get; set; }

    /// <summary>
    /// 缓冲区的透明度。
    /// The opacity of the buffer track.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? BufferOpacity { get; set; }

    /// <summary>
    /// 是否允许点击进度条。
    /// Whether the progress bar is clickable.
    /// </summary>
    [Parameter]
    public bool Clickable { get; set; }

    /// <summary>
    /// 进度条的高度。
    /// The height of the progress bar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否显示为不确定状态的动画。
    /// Whether to display an indeterminate animation.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// 进度的最大值。
    /// The maximum value of the progress.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 当前进度值。
    /// The current progress value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ModelValue { get; set; }

    /// <summary>
    /// 进度值变更时触发的回调。
    /// Callback invoked when the progress value changes.
    /// </summary>
    [Parameter]
    public EventCallback<Number> ModelValueChanged { get; set; }

    /// <summary>
    /// 进度条的透明度。
    /// The opacity of the progress bar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 是否反转进度条方向。
    /// Whether to reverse the progress bar direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否显示流式动画效果。
    /// Whether to show a streaming animation effect.
    /// </summary>
    [Parameter]
    public bool Stream { get; set; }

    /// <summary>
    /// 是否显示条纹效果。
    /// Whether to show a striped effect.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// 进度条是否显示圆角。
    /// Whether the progress bar track has rounded corners.
    /// </summary>
    [Parameter]
    public bool RoundedBar { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VProgressLinearDefaultSlotContext>? ChildContent { get; set; }
}
