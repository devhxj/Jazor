namespace ECMAScript.VueDataUi;

/// <summary>Vue Data UI 基础 chart 的 Razor component catalog。其余公开组件在
/// <c>VueDataUiExtendedComponents</c> 中；每个 descriptor 均指向单独 ESM entry，
/// so an authored chart never imports the package root bundle.</summary>
[ECMAScriptName("VueUiXy")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-xy")]
public sealed class VdXy : VdChartComponent<VdXyDatasetItem[], VdXyConfig>
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
[ECMAScriptName("VueUiDonut")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-donut")]
public sealed class VdDonut : VdChartComponent<VdDonutDatasetItem[], VdDonutConfig>
{
    /// <summary>
    /// 图例选择状态变化时触发；携带当前选中的图例项。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onSelectLegend")]
    public EventCallback<VdDonutLegendItem[]> OnSelectLegend { get; set; }
}

/// <summary>
/// 在仪表盘刻度上显示当前值及多个区间。
/// </summary>
[ECMAScriptName("VueUiGauge")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-gauge")]
public sealed class VdGauge : VdChartComponent<VdGaugeDataset, VdGaugeConfig>;

/// <summary>
/// 使用竖直条形比较各分类数值，支持子项细分。
/// </summary>
[ECMAScriptName("VueUiVerticalBar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-vertical-bar")]
public sealed class VdVerticalBar : VdChartComponent<VdVerticalBarDatasetItem[], VdVerticalBarConfig>;

/// <summary>
/// 使用水平条形比较各分类数值。
/// </summary>
[ECMAScriptName("VueUiHorizontalBar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-horizontal-bar")]
public sealed class VdHorizontalBar : VdChartComponent<VdHorizontalBarDatasetItem[], VdHorizontalBarConfig>;

/// <summary>
/// 按分类堆叠多条数据序列，显示总量及组成。
/// </summary>
[ECMAScriptName("VueUiStackbar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-stackbar")]
public sealed class VdStackbar : VdChartComponent<VdStackbarDatasetItem[], VdStackbarConfig>;

/// <summary>
/// 将多条折线或面积序列叠加显示，适合比较随时间变化的组成。
/// </summary>
[ECMAScriptName("VueUiStackline")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-stackline")]
public sealed class VdStackline : VdChartComponent<VdStacklineDatasetItem[], VdStacklineConfig>
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
[ECMAScriptName("VueUiSparkline")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-sparkline")]
public sealed class VdSparkline : VdChartComponent<VdSparklineDatasetItem[], VdSparklineConfig>;

/// <summary>
/// 以紧凑条形展示分类数值。
/// </summary>
[ECMAScriptName("VueUiSparkbar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-sparkbar")]
public sealed class VdSparkbar : VdChartComponent<VdSparkbarDatasetItem[], VdSparkbarConfig>;

/// <summary>
/// 以紧凑直方图展示各时间段的数值。
/// </summary>
[ECMAScriptName("VueUiSparkHistogram")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-sparkhistogram")]
public sealed class VdSparkHistogram : VdChartComponent<VdSparkHistogramDatasetItem[], VdSparkHistogramConfig>;

/// <summary>
/// 沿多个分类轴比较一条或多条数据序列。
/// </summary>
[ECMAScriptName("VueUiRadar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-radar")]
public sealed class VdRadar : VdChartComponent<VdRadarDataset, VdRadarConfig>;

/// <summary>
/// 以方格填充比例展示各数据项的占比。
/// </summary>
[ECMAScriptName("VueUiWaffle")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-waffle")]
public sealed class VdWaffle : VdChartComponent<VdWaffleDatasetItem[], VdWaffleConfig>;

/// <summary>
/// 使用嵌套矩形的面积展示层级数据的数值关系。
/// </summary>
[ECMAScriptName("VueUiTreemap")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-treemap")]
public sealed class VdTreemap : VdChartComponent<VdTreemapDatasetItem[], VdTreemapConfig>;

/// <summary>
/// 使用颜色强度展示二维分类中的数值大小。
/// </summary>
[ECMAScriptName("VueUiHeatmap")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-heatmap")]
public sealed class VdHeatmap : VdChartComponent<VdHeatmapDatasetItem[], VdHeatmapConfig>;

/// <summary>
/// 根据 X/Y 坐标绘制散点，支持权重和多序列。
/// </summary>
[ECMAScriptName("VueUiScatter")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-scatter")]
public sealed class VdScatter : VdChartComponent<VdScatterDatasetItem[], VdScatterConfig>;

/// <summary>
/// 显示依次缩减或变化的各阶段数值。
/// </summary>
[ECMAScriptName("VueUiFunnel")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-funnel")]
public sealed class VdFunnel : VdChartComponent<VdFunnelDatasetItem[], VdFunnelConfig>;

/// <summary>
/// 根据词条权重绘制大小不同的文字。
/// </summary>
[ECMAScriptName("VueUiWordCloud")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-word-cloud")]
public sealed class VdWordCloud : VdChartComponent<VdWordCloudDataset, VdWordCloudConfig>;

/// <summary>
/// 显示关键指标数值，支持前后缀、舍入和变化动画。
/// </summary>
[ECMAScriptName("VueUiKpi")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-kpi")]
public sealed class VdKpi : VdChartComponent<double, VdKpiConfig>;

/// <summary>
/// 显示可配置排序、搜索、汇总和分页的数据表格。
/// </summary>
[ECMAScriptName("VueUiTable")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-table")]
public sealed class VdTable : VdChartComponent<VdTableDataset, VdTableConfig>;

/// <summary>
/// 在表格单元格中以颜色强度表达数值差异。
/// </summary>
[ECMAScriptName("VueUiTableHeatmap")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-table-heatmap")]
public sealed class VdTableHeatmap : VdChartComponent<VdTableHeatmapDatasetItem[], VdTableHeatmapConfig>;

/// <summary>
/// 在表格行中显示序列的小型趋势图。
/// </summary>
[ECMAScriptName("VueUiTableSparkline")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-table-sparkline")]
public sealed class VdTableSparkline : VdRequiredConfigChartComponent<VdTableSparklineDatasetItem[], VdTableSparklineConfig>;

/// <summary>
/// 根据简化配置快速呈现图表，支持图例和提示开关。
/// </summary>
[ECMAScriptName("VueUiQuickChart")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-quick-chart")]
public sealed class VdQuickChart : VdChartComponent<VdQuickChartDataset, VdQuickChartConfig>;

/// <summary>
/// 展示开盘、最高、最低、收盘及成交量等金融时间序列。
/// </summary>
[ECMAScriptName("VueUiCandlestick")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-candlestick")]
public sealed class VdCandlestick : VdChartComponent<VdCellValue[][], VdCandlestickConfig>
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
[ECMAScriptName("VueUiDumbbell")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-dumbbell")]
public sealed class VdDumbbell : VdChartComponent<VdDumbbellDataset[], VdDumbbellConfig>;

/// <summary>
/// 将实际值、目标值与背景区间放在同一刻度上比较。
/// </summary>
[ECMAScriptName("VueUiBullet")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-bullet")]
public sealed class VdBullet : VdChartComponent<VdBulletDataset, VdBulletConfig>;
