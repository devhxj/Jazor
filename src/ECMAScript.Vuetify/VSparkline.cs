using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 迷你图表组件的编写代理，用于紧凑趋势和柱状可视化。
/// Vuetify sparkline authoring proxy for compact trend and bar visualizations.
/// </summary>
[ECMAScript("vuetify/components/VSparkline")]
public sealed class VSparkline : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否在挂载时自动绘制动画。
    /// Whether to animate the drawing on mount.
    /// </summary>
    [Parameter]
    [ECMAScriptName("autoDraw")]
    public bool AutoDraw { get; set; }

    /// <summary>
    /// 自动绘制动画的持续毫秒数。
    /// Duration in milliseconds of the auto-draw animation.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 AutoDrawDuration="@(32)"；变量用 AutoDrawDuration="@value"，无需 double 后缀。字符串用 AutoDrawDuration="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("autoDrawDuration")]
    public VueStringNumberValue? AutoDrawDuration { get; set; }

    /// <summary>
    /// 自动绘制动画的缓动函数名称。
    /// Easing function name for the auto-draw animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("autoDrawEasing")]
    public string? AutoDrawEasing { get; set; }

    /// <summary>
    /// 迷你图表的线条颜色。
    /// Line color of the sparkline.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 渐变色的颜色列表。
    /// List of colors for the gradient.
    /// </summary>
    [Parameter]
    [ECMAScriptName("gradient")]
    public string[]? Gradient { get; set; }

    /// <summary>
    /// 渐变色的方向。
    /// Direction of the gradient.
    /// </summary>
    [Parameter]
    [ECMAScriptName("gradientDirection")]
    public VuetifySparklineGradientDirection? GradientDirection { get; set; }

    /// <summary>
    /// 迷你图表的高度。
    /// Height of the sparkline.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 坐标轴标签的文本列表。
    /// List of label texts for the axis.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySparklineItems?；值域为 VuetifySparklineItem[]。变量用 Labels="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("labels")]
    public VuetifySparklineItems? Labels { get; set; }

    /// <summary>
    /// 标签的字体大小。
    /// Font size of the labels.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 LabelSize="@(32)"；变量用 LabelSize="@value"，无需 double 后缀。字符串用 LabelSize="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("labelSize")]
    public VueStringNumberValue? LabelSize { get; set; }

    /// <summary>
    /// 线条的宽度。
    /// Width of the line stroke.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 LineWidth="@(32)"；变量用 LineWidth="@value"，无需 double 后缀。字符串用 LineWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("lineWidth")]
    public VueStringNumberValue? LineWidth { get; set; }

    /// <summary>
    /// 组件的唯一标识符。
    /// Unique identifier of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 数据项中取值的属性名。
    /// Property name to extract value from each data item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public string? ItemValue { get; set; }

    /// <summary>
    /// 迷你图表的数据数组。
    /// Data array for the sparkline.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySparklineItems?；值域为 VuetifySparklineItem[]。变量用 ModelValue="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySparklineItems? ModelValue { get; set; }

    /// <summary>
    /// Y 轴的最小值。
    /// Minimum value of the Y-axis.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Min="@(32)"；变量用 Min="@value"，无需 double 后缀。字符串用 Min="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("min")]
    public VueStringNumberValue? Min { get; set; }

    /// <summary>
    /// Y 轴的最大值。
    /// Maximum value of the Y-axis.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 图表与边缘的内边距。
    /// Padding between the chart and edges.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Padding="@(32)"；变量用 Padding="@value"，无需 double 后缀。字符串用 Padding="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("padding")]
    public VueStringNumberValue? Padding { get; set; }

    /// <summary>
    /// 是否显示坐标轴标签。
    /// Whether to show axis labels.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showLabels")]
    public bool ShowLabels { get; set; }

    /// <summary>
    /// 线条的平滑度。
    /// Smoothness of the line curve.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySparklineSmoothValue?；值域为 bool | Number | string。数值用 Smooth="@(32)"；变量用 Smooth="@value"，无需 double 后缀。字符串用 Smooth="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("smooth")]
    public VuetifySparklineSmoothValue? Smooth { get; set; }

    /// <summary>
    /// 迷你图表的宽度。
    /// Width of the sparkline.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 是否填充线条下方区域。
    /// Whether to fill the area under the line.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fill")]
    public bool Fill { get; set; }

    /// <summary>
    /// 是否根据容器宽度自动计算线宽。
    /// Whether to auto-calculate line width based on container width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("autoLineWidth")]
    public bool AutoLineWidth { get; set; }

    /// <summary>
    /// 迷你图表的类型（折线或柱状）。
    /// Type of the sparkline (line or bar).
    /// </summary>
    [Parameter]
    [ECMAScriptName("type")]
    public VuetifySparklineType? Type { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，迷你图表的自定义内容。
    /// Default slot for custom sparkline content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 标签插槽，自定义每个标签的渲染。
    /// Label slot for customizing each label rendering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public RenderFragment<VSparklineLabelSlotContext>? Label { get; set; }
}
