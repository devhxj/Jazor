using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 迷你图表组件的编写代理，用于紧凑趋势和柱状可视化。
/// Vuetify sparkline authoring proxy for compact trend and bar visualizations.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSparkline", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VSparkline : ComponentBase
{
    /// <summary>
    /// 是否在挂载时自动绘制动画。
    /// Whether to animate the drawing on mount.
    /// </summary>
    [Parameter]
    public bool AutoDraw { get; set; }

    /// <summary>
    /// 自动绘制动画的持续毫秒数。
    /// Duration in milliseconds of the auto-draw animation.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? AutoDrawDuration { get; set; }

    /// <summary>
    /// 自动绘制动画的缓动函数名称。
    /// Easing function name for the auto-draw animation.
    /// </summary>
    [Parameter]
    public string? AutoDrawEasing { get; set; }

    /// <summary>
    /// 迷你图表的线条颜色。
    /// Line color of the sparkline.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 渐变色的颜色列表。
    /// List of colors for the gradient.
    /// </summary>
    [Parameter]
    public string[]? Gradient { get; set; }

    /// <summary>
    /// 渐变色的方向。
    /// Direction of the gradient.
    /// </summary>
    [Parameter]
    public VuetifySparklineGradientDirection? GradientDirection { get; set; }

    /// <summary>
    /// 迷你图表的高度。
    /// Height of the sparkline.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 坐标轴标签的文本列表。
    /// List of label texts for the axis.
    /// </summary>
    [Parameter]
    public VuetifySparklineItems? Labels { get; set; }

    /// <summary>
    /// 标签的字体大小。
    /// Font size of the labels.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? LabelSize { get; set; }

    /// <summary>
    /// 线条的宽度。
    /// Width of the line stroke.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? LineWidth { get; set; }

    /// <summary>
    /// 组件的唯一标识符。
    /// Unique identifier of the component.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 数据项中取值的属性名。
    /// Property name to extract value from each data item.
    /// </summary>
    [Parameter]
    public string? ItemValue { get; set; }

    /// <summary>
    /// 迷你图表的数据数组。
    /// Data array for the sparkline.
    /// </summary>
    [Parameter]
    public VuetifySparklineItems? ModelValue { get; set; }

    /// <summary>
    /// Y 轴的最小值。
    /// Minimum value of the Y-axis.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Min { get; set; }

    /// <summary>
    /// Y 轴的最大值。
    /// Maximum value of the Y-axis.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 图表与边缘的内边距。
    /// Padding between the chart and edges.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Padding { get; set; }

    /// <summary>
    /// 是否显示坐标轴标签。
    /// Whether to show axis labels.
    /// </summary>
    [Parameter]
    public bool ShowLabels { get; set; }

    /// <summary>
    /// 线条的平滑度。
    /// Smoothness of the line curve.
    /// </summary>
    [Parameter]
    public VuetifySparklineSmoothValue? Smooth { get; set; }

    /// <summary>
    /// 迷你图表的宽度。
    /// Width of the sparkline.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 是否填充线条下方区域。
    /// Whether to fill the area under the line.
    /// </summary>
    [Parameter]
    public bool Fill { get; set; }

    /// <summary>
    /// 是否根据容器宽度自动计算线宽。
    /// Whether to auto-calculate line width based on container width.
    /// </summary>
    [Parameter]
    public bool AutoLineWidth { get; set; }

    /// <summary>
    /// 迷你图表的类型（折线或柱状）。
    /// Type of the sparkline (line or bar).
    /// </summary>
    [Parameter]
    public VuetifySparklineType? Type { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，迷你图表的自定义内容。
    /// Default slot for custom sparkline content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 标签插槽，自定义每个标签的渲染。
    /// Label slot for customizing each label rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<VSparklineLabelSlotContext>? Label { get; set; }
}
