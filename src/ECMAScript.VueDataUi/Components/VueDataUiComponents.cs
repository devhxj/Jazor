namespace ECMAScript.VueDataUi;

/// <summary>Vue Data UI 基础 chart 的 Razor component catalog。其余公开组件在
/// <c>VueDataUiExtendedComponents</c> 中；每个 descriptor 均指向单独 ESM entry，
/// so an authored chart never imports the package root bundle.</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-xy", "VueUiXy")]
public sealed class VueUiXy : VueDataUiChartComponent<VueUiXyDatasetItem[], VueUiXyConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[VueLibraryComponent("vue-data-ui/vue-ui-donut", "VueUiDonut")]
public sealed class VueUiDonut : VueDataUiChartComponent<VueUiDonutDatasetItem[], VueUiDonutConfig>
{
    [Parameter]
    [ECMAScriptName("onSelectLegend")]
    public EventCallback<VueUiDonutLegendItem[]> OnSelectLegend { get; set; }
}

[VueLibraryComponent("vue-data-ui/vue-ui-gauge", "VueUiGauge")]
public sealed class VueUiGauge : VueDataUiChartComponent<VueUiGaugeDataset, VueUiGaugeConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-vertical-bar", "VueUiVerticalBar")]
public sealed class VueUiVerticalBar : VueDataUiChartComponent<VueUiVerticalBarDatasetItem[], VueUiVerticalBarConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-horizontal-bar", "VueUiHorizontalBar")]
public sealed class VueUiHorizontalBar : VueDataUiChartComponent<VueUiHorizontalBarDatasetItem[], VueUiHorizontalBarConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-stackbar", "VueUiStackbar")]
public sealed class VueUiStackbar : VueDataUiChartComponent<VueUiStackbarDatasetItem[], VueUiStackbarConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-stackline", "VueUiStackline")]
public sealed class VueUiStackline : VueDataUiChartComponent<VueUiStacklineDatasetItem[], VueUiStacklineConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[VueLibraryComponent("vue-data-ui/vue-ui-sparkline", "VueUiSparkline")]
public sealed class VueUiSparkline : VueDataUiChartComponent<VueUiSparklineDatasetItem[], VueUiSparklineConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-sparkbar", "VueUiSparkbar")]
public sealed class VueUiSparkbar : VueDataUiChartComponent<VueUiSparkbarDatasetItem[], VueUiSparkbarConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-sparkhistogram", "VueUiSparkHistogram")]
public sealed class VueUiSparkHistogram : VueDataUiChartComponent<VueUiSparkHistogramDatasetItem[], VueUiSparkHistogramConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-radar", "VueUiRadar")]
public sealed class VueUiRadar : VueDataUiChartComponent<VueUiRadarDataset, VueUiRadarConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-waffle", "VueUiWaffle")]
public sealed class VueUiWaffle : VueDataUiChartComponent<VueUiWaffleDatasetItem[], VueUiWaffleConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-treemap", "VueUiTreemap")]
public sealed class VueUiTreemap : VueDataUiChartComponent<VueUiTreemapDatasetItem[], VueUiTreemapConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-heatmap", "VueUiHeatmap")]
public sealed class VueUiHeatmap : VueDataUiChartComponent<VueUiHeatmapDatasetItem[], VueUiHeatmapConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-scatter", "VueUiScatter")]
public sealed class VueUiScatter : VueDataUiChartComponent<VueUiScatterDatasetItem[], VueUiScatterConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-funnel", "VueUiFunnel")]
public sealed class VueUiFunnel : VueDataUiChartComponent<VueUiFunnelDatasetItem[], VueUiFunnelConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-word-cloud", "VueUiWordCloud")]
public sealed class VueUiWordCloud : VueDataUiChartComponent<VueUiWordCloudDataset, VueUiWordCloudConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-kpi", "VueUiKpi")]
public sealed class VueUiKpi : VueDataUiChartComponent<double, VueUiKpiConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-table", "VueUiTable")]
public sealed class VueUiTable : VueDataUiChartComponent<VueUiTableDataset, VueUiTableConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-table-heatmap", "VueUiTableHeatmap")]
public sealed class VueUiTableHeatmap : VueDataUiChartComponent<VueUiTableHeatmapDatasetItem[], VueUiTableHeatmapConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-table-sparkline", "VueUiTableSparkline")]
public sealed class VueUiTableSparkline : VueDataUiRequiredConfigChartComponent<VueUiTableSparklineDatasetItem[], VueUiTableSparklineConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-quick-chart", "VueUiQuickChart")]
public sealed class VueUiQuickChart : VueDataUiChartComponent<VueUiQuickChartDataset, VueUiQuickChartConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-candlestick", "VueUiCandlestick")]
public sealed class VueUiCandlestick : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiCandlestickConfig>
{
    [Parameter]
    [ECMAScriptName("selectedXIndex")]
    public int? SelectedXIndex { get; set; }
}

[VueLibraryComponent("vue-data-ui/vue-ui-dumbbell", "VueUiDumbbell")]
public sealed class VueUiDumbbell : VueDataUiChartComponent<VueUiDumbbellDataset[], VueUiDumbbellConfig>;

[VueLibraryComponent("vue-data-ui/vue-ui-bullet", "VueUiBullet")]
public sealed class VueUiBullet : VueDataUiChartComponent<VueUiBulletDataset, VueUiBulletConfig>;
