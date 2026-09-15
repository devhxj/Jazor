namespace ECMAScript.VueDataUi;

/// <summary>Vue Data UI 基础 chart 的 Razor component catalog。其余公开组件在
/// <c>VueDataUiExtendedComponents</c> 中；每个 descriptor 均指向单独 ESM entry，
/// so an authored chart never imports the package root bundle.</summary>
[ECMAScript("vue-data-ui/vue-ui-xy", Transform.Component, "VueUiXy")]
public sealed class VueUiXy : VueDataUiChartComponent<VueUiXyDatasetItem[], VueUiXyConfig>
{
    /// <summary>
    /// 外部控制的 X 轴选中索引，从 0 开始；可用于同步多个图表的指示位置。
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

/// <summary>
/// 以环形扇区显示各项的数值占比，支持图例和数据提示。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-donut", Transform.Component, "VueUiDonut")]
public sealed class VueUiDonut : VueDataUiChartComponent<VueUiDonutDatasetItem[], VueUiDonutConfig>
{
    /// <summary>
    /// 图例选择状态变化时触发；携带当前选中的图例项。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onSelectLegend")]
    public EventCallback<VueUiDonutLegendItem[]> OnSelectLegend { get; set; }
}

/// <summary>
/// 在仪表盘刻度上显示当前值及多个区间。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-gauge", Transform.Component, "VueUiGauge")]
public sealed class VueUiGauge : VueDataUiChartComponent<VueUiGaugeDataset, VueUiGaugeConfig>;

/// <summary>
/// 使用竖直条形比较各分类数值，支持子项细分。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-vertical-bar", Transform.Component, "VueUiVerticalBar")]
public sealed class VueUiVerticalBar : VueDataUiChartComponent<VueUiVerticalBarDatasetItem[], VueUiVerticalBarConfig>;

/// <summary>
/// 使用水平条形比较各分类数值。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-horizontal-bar", Transform.Component, "VueUiHorizontalBar")]
public sealed class VueUiHorizontalBar : VueDataUiChartComponent<VueUiHorizontalBarDatasetItem[], VueUiHorizontalBarConfig>;

/// <summary>
/// 按分类堆叠多条数据序列，显示总量及组成。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-stackbar", Transform.Component, "VueUiStackbar")]
public sealed class VueUiStackbar : VueDataUiChartComponent<VueUiStackbarDatasetItem[], VueUiStackbarConfig>;

/// <summary>
/// 将多条折线或面积序列叠加显示，适合比较随时间变化的组成。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-stackline", Transform.Component, "VueUiStackline")]
public sealed class VueUiStackline : VueDataUiChartComponent<VueUiStacklineDatasetItem[], VueUiStacklineConfig>
{
    /// <summary>
    /// 外部控制的 X 轴选中索引，从 0 开始；可用于同步多个图表的指示位置。
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

/// <summary>
/// 以紧凑折线显示一组按时间或分类排列的数据。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-sparkline", Transform.Component, "VueUiSparkline")]
public sealed class VueUiSparkline : VueDataUiChartComponent<VueUiSparklineDatasetItem[], VueUiSparklineConfig>;

/// <summary>
/// 以紧凑条形展示分类数值。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-sparkbar", Transform.Component, "VueUiSparkbar")]
public sealed class VueUiSparkbar : VueDataUiChartComponent<VueUiSparkbarDatasetItem[], VueUiSparkbarConfig>;

/// <summary>
/// 以紧凑直方图展示各时间段的数值。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-sparkhistogram", Transform.Component, "VueUiSparkHistogram")]
public sealed class VueUiSparkHistogram : VueDataUiChartComponent<VueUiSparkHistogramDatasetItem[], VueUiSparkHistogramConfig>;

/// <summary>
/// 沿多个分类轴比较一条或多条数据序列。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-radar", Transform.Component, "VueUiRadar")]
public sealed class VueUiRadar : VueDataUiChartComponent<VueUiRadarDataset, VueUiRadarConfig>;

/// <summary>
/// 以方格填充比例展示各数据项的占比。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-waffle", Transform.Component, "VueUiWaffle")]
public sealed class VueUiWaffle : VueDataUiChartComponent<VueUiWaffleDatasetItem[], VueUiWaffleConfig>;

/// <summary>
/// 使用嵌套矩形的面积展示层级数据的数值关系。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-treemap", Transform.Component, "VueUiTreemap")]
public sealed class VueUiTreemap : VueDataUiChartComponent<VueUiTreemapDatasetItem[], VueUiTreemapConfig>;

/// <summary>
/// 使用颜色强度展示二维分类中的数值大小。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-heatmap", Transform.Component, "VueUiHeatmap")]
public sealed class VueUiHeatmap : VueDataUiChartComponent<VueUiHeatmapDatasetItem[], VueUiHeatmapConfig>;

/// <summary>
/// 根据 X/Y 坐标绘制散点，支持权重和多序列。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-scatter", Transform.Component, "VueUiScatter")]
public sealed class VueUiScatter : VueDataUiChartComponent<VueUiScatterDatasetItem[], VueUiScatterConfig>;

/// <summary>
/// 显示依次缩减或变化的各阶段数值。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-funnel", Transform.Component, "VueUiFunnel")]
public sealed class VueUiFunnel : VueDataUiChartComponent<VueUiFunnelDatasetItem[], VueUiFunnelConfig>;

/// <summary>
/// 根据词条权重绘制大小不同的文字。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-word-cloud", Transform.Component, "VueUiWordCloud")]
public sealed class VueUiWordCloud : VueDataUiChartComponent<VueUiWordCloudDataset, VueUiWordCloudConfig>;

/// <summary>
/// 显示关键指标数值，支持前后缀、舍入和变化动画。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-kpi", Transform.Component, "VueUiKpi")]
public sealed class VueUiKpi : VueDataUiChartComponent<double, VueUiKpiConfig>;

/// <summary>
/// 显示可配置排序、搜索、汇总和分页的数据表格。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-table", Transform.Component, "VueUiTable")]
public sealed class VueUiTable : VueDataUiChartComponent<VueUiTableDataset, VueUiTableConfig>;

/// <summary>
/// 在表格单元格中以颜色强度表达数值差异。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-table-heatmap", Transform.Component, "VueUiTableHeatmap")]
public sealed class VueUiTableHeatmap : VueDataUiChartComponent<VueUiTableHeatmapDatasetItem[], VueUiTableHeatmapConfig>;

/// <summary>
/// 在表格行中显示序列的小型趋势图。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-table-sparkline", Transform.Component, "VueUiTableSparkline")]
public sealed class VueUiTableSparkline : VueDataUiRequiredConfigChartComponent<VueUiTableSparklineDatasetItem[], VueUiTableSparklineConfig>;

/// <summary>
/// 根据简化配置快速呈现图表，支持图例和提示开关。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-quick-chart", Transform.Component, "VueUiQuickChart")]
public sealed class VueUiQuickChart : VueDataUiChartComponent<VueUiQuickChartDataset, VueUiQuickChartConfig>;

/// <summary>
/// 展示开盘、最高、最低、收盘及成交量等金融时间序列。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-candlestick", Transform.Component, "VueUiCandlestick")]
public sealed class VueUiCandlestick : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiCandlestickConfig>
{
    /// <summary>
    /// 外部控制的 X 轴选中索引，从 0 开始；可用于同步多个图表的指示位置。
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

/// <summary>
/// 以两端点及连接线比较每项的起始和结束数值。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-dumbbell", Transform.Component, "VueUiDumbbell")]
public sealed class VueUiDumbbell : VueDataUiChartComponent<VueUiDumbbellDataset[], VueUiDumbbellConfig>;

/// <summary>
/// 将实际值、目标值与背景区间放在同一刻度上比较。
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-bullet", Transform.Component, "VueUiBullet")]
public sealed class VueUiBullet : VueDataUiChartComponent<VueUiBulletDataset, VueUiBulletConfig>;