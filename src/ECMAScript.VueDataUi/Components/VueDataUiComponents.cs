namespace ECMAScript.VueDataUi;

/// <summary>Vue Data UI 基础 chart 的 Razor component catalog。其余公开组件在
/// <c>VueDataUiExtendedComponents</c> 中；每个 descriptor 均指向单独 ESM entry，
/// so an authored chart never imports the package root bundle.</summary>
[ECMAScript("vue-data-ui/vue-ui-xy", Transform.Component, "VueUiXy")]
public sealed class VueUiXy : VueDataUiChartComponent<VueUiXyDatasetItem[], VueUiXyConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[ECMAScript("vue-data-ui/vue-ui-donut", Transform.Component, "VueUiDonut")]
public sealed class VueUiDonut : VueDataUiChartComponent<VueUiDonutDatasetItem[], VueUiDonutConfig>
{
    [Parameter]
    [ECMAScriptName("onSelectLegend")]
    public EventCallback<VueUiDonutLegendItem[]> OnSelectLegend { get; set; }
}

[ECMAScript("vue-data-ui/vue-ui-gauge", Transform.Component, "VueUiGauge")]
public sealed class VueUiGauge : VueDataUiChartComponent<VueUiGaugeDataset, VueUiGaugeConfig>;

[ECMAScript("vue-data-ui/vue-ui-vertical-bar", Transform.Component, "VueUiVerticalBar")]
public sealed class VueUiVerticalBar : VueDataUiChartComponent<VueUiVerticalBarDatasetItem[], VueUiVerticalBarConfig>;

[ECMAScript("vue-data-ui/vue-ui-horizontal-bar", Transform.Component, "VueUiHorizontalBar")]
public sealed class VueUiHorizontalBar : VueDataUiChartComponent<VueUiHorizontalBarDatasetItem[], VueUiHorizontalBarConfig>;

[ECMAScript("vue-data-ui/vue-ui-stackbar", Transform.Component, "VueUiStackbar")]
public sealed class VueUiStackbar : VueDataUiChartComponent<VueUiStackbarDatasetItem[], VueUiStackbarConfig>;

[ECMAScript("vue-data-ui/vue-ui-stackline", Transform.Component, "VueUiStackline")]
public sealed class VueUiStackline : VueDataUiChartComponent<VueUiStacklineDatasetItem[], VueUiStacklineConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[ECMAScript("vue-data-ui/vue-ui-sparkline", Transform.Component, "VueUiSparkline")]
public sealed class VueUiSparkline : VueDataUiChartComponent<VueUiSparklineDatasetItem[], VueUiSparklineConfig>;

[ECMAScript("vue-data-ui/vue-ui-sparkbar", Transform.Component, "VueUiSparkbar")]
public sealed class VueUiSparkbar : VueDataUiChartComponent<VueUiSparkbarDatasetItem[], VueUiSparkbarConfig>;

[ECMAScript("vue-data-ui/vue-ui-sparkhistogram", Transform.Component, "VueUiSparkHistogram")]
public sealed class VueUiSparkHistogram : VueDataUiChartComponent<VueUiSparkHistogramDatasetItem[], VueUiSparkHistogramConfig>;

[ECMAScript("vue-data-ui/vue-ui-radar", Transform.Component, "VueUiRadar")]
public sealed class VueUiRadar : VueDataUiChartComponent<VueUiRadarDataset, VueUiRadarConfig>;

[ECMAScript("vue-data-ui/vue-ui-waffle", Transform.Component, "VueUiWaffle")]
public sealed class VueUiWaffle : VueDataUiChartComponent<VueUiWaffleDatasetItem[], VueUiWaffleConfig>;

[ECMAScript("vue-data-ui/vue-ui-treemap", Transform.Component, "VueUiTreemap")]
public sealed class VueUiTreemap : VueDataUiChartComponent<VueUiTreemapDatasetItem[], VueUiTreemapConfig>;

[ECMAScript("vue-data-ui/vue-ui-heatmap", Transform.Component, "VueUiHeatmap")]
public sealed class VueUiHeatmap : VueDataUiChartComponent<VueUiHeatmapDatasetItem[], VueUiHeatmapConfig>;

[ECMAScript("vue-data-ui/vue-ui-scatter", Transform.Component, "VueUiScatter")]
public sealed class VueUiScatter : VueDataUiChartComponent<VueUiScatterDatasetItem[], VueUiScatterConfig>;

[ECMAScript("vue-data-ui/vue-ui-funnel", Transform.Component, "VueUiFunnel")]
public sealed class VueUiFunnel : VueDataUiChartComponent<VueUiFunnelDatasetItem[], VueUiFunnelConfig>;

[ECMAScript("vue-data-ui/vue-ui-word-cloud", Transform.Component, "VueUiWordCloud")]
public sealed class VueUiWordCloud : VueDataUiChartComponent<VueUiWordCloudDataset, VueUiWordCloudConfig>;

[ECMAScript("vue-data-ui/vue-ui-kpi", Transform.Component, "VueUiKpi")]
public sealed class VueUiKpi : VueDataUiChartComponent<double, VueUiKpiConfig>;

[ECMAScript("vue-data-ui/vue-ui-table", Transform.Component, "VueUiTable")]
public sealed class VueUiTable : VueDataUiChartComponent<VueUiTableDataset, VueUiTableConfig>;

[ECMAScript("vue-data-ui/vue-ui-table-heatmap", Transform.Component, "VueUiTableHeatmap")]
public sealed class VueUiTableHeatmap : VueDataUiChartComponent<VueUiTableHeatmapDatasetItem[], VueUiTableHeatmapConfig>;

[ECMAScript("vue-data-ui/vue-ui-table-sparkline", Transform.Component, "VueUiTableSparkline")]
public sealed class VueUiTableSparkline : VueDataUiRequiredConfigChartComponent<VueUiTableSparklineDatasetItem[], VueUiTableSparklineConfig>;

[ECMAScript("vue-data-ui/vue-ui-quick-chart", Transform.Component, "VueUiQuickChart")]
public sealed class VueUiQuickChart : VueDataUiChartComponent<VueUiQuickChartDataset, VueUiQuickChartConfig>;

[ECMAScript("vue-data-ui/vue-ui-candlestick", Transform.Component, "VueUiCandlestick")]
public sealed class VueUiCandlestick : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiCandlestickConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[ECMAScript("vue-data-ui/vue-ui-dumbbell", Transform.Component, "VueUiDumbbell")]
public sealed class VueUiDumbbell : VueDataUiChartComponent<VueUiDumbbellDataset[], VueUiDumbbellConfig>;

[ECMAScript("vue-data-ui/vue-ui-bullet", Transform.Component, "VueUiBullet")]
public sealed class VueUiBullet : VueDataUiChartComponent<VueUiBulletDataset, VueUiBulletConfig>;
