namespace ECMAScript.VueDataUi;

/// <summary>
/// vue-data-ui 3.23.4 完整公开 catalog 的其余 Razor descriptors。Each type imports its own
/// <c>vue-data-ui/vue-ui-*</c> entry so a component never pulls the aggregate root bundle.
/// </summary>
[VueLibraryComponent("vue-data-ui/vue-ui-3d-bar", "VueUi3dBar")]
public sealed class VueUi3dBar : VueDataUiChartComponent<VueUi3dBarDataset, VueUi3dBarConfig>;

/// <summary>可折叠 content 容器。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-accordion", "VueUiAccordion")]
public sealed class VueUiAccordion : VueDataUiConfigComponent<VueUiAccordionConfig>
{
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public bool? HideDetails { get; set; }
}

/// <summary>年龄金字塔图表。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-age-pyramid", "VueUiAgePyramid")]
public sealed class VueUiAgePyramid : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiAgePyramidConfig>;

/// <summary>图表 annotation overlay。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-annotator", "VueUiAnnotator")]
public sealed class VueUiAnnotator : VueDataUiOptionalDatasetChartComponent<VueUiAnnotatorDataset, VueUiAnnotatorConfig>;

/// <summary>排名变化的 bump chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-bump", "VueUiBump")]
public sealed class VueUiBump : VueDataUiChartComponent<VueUiBumpDatasetItem[], VueUiBumpConfig>;

/// <summary>自动滚动 carousel table。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-carousel-table", "VueUiCarouselTable")]
public sealed class VueUiCarouselTable : VueDataUiChartComponent<VueUiCarouselTableDataset, VueUiCarouselTableConfig>;

/// <summary>层级 chestnut chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-chestnut", "VueUiChestnut")]
public sealed class VueUiChestnut : VueDataUiChartComponent<VueUiChestnutDatasetRoot[], VueUiChestnutConfig>;

/// <summary>矩阵关系 chord chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-chord", "VueUiChord")]
public sealed class VueUiChord : VueDataUiChartComponent<VueUiChordDataset, VueUiChordConfig>;

/// <summary>层级 circle pack chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-circle-pack", "VueUiCirclePack")]
public sealed class VueUiCirclePack : VueDataUiChartComponent<VueUiCirclePackDatasetItem[], VueUiCirclePackConfig>;

/// <summary>交叉坐标 cursor overlay。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-cursor", "VueUiCursor")]
public sealed class VueUiCursor : VueDataUiConfigComponent<VueUiCursorConfig>;

/// <summary>有向无环图 visualization。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-dag", "VueUiDag")]
public sealed class VueUiDag : VueDataUiChartComponent<VueUiDagDataset, VueUiDagConfig>;

/// <summary>可拖动 chart dashboard layout。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-dashboard", "VueUiDashboard")]
public sealed class VueUiDashboard : VueDataUiChartComponent<VueUiDashboardElement[], VueUiDashboardConfig>
{
    [Parameter]
    [ECMAScriptName("onChange")]
    public EventCallback<VueUiDashboardPlacedElement[]> OnChange { get; set; }

    [Parameter]
    [ECMAScriptName("onCopyAlt")]
    public EventCallback<VueUiDashboardCopyAlt> OnCopyAlt { get; set; }
}

/// <summary>数码管数值 visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-digits", "VueUiDigits")]
public sealed class VueUiDigits : VueDataUiChartComponent<double, VueUiDigitsConfig>;

/// <summary>多时段 donut evolution chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-donut-evolution", "VueUiDonutEvolution")]
public sealed class VueUiDonutEvolution : VueDataUiChartComponent<VueUiDonutEvolutionDatasetItem[], VueUiDonutEvolutionConfig>;

/// <summary>source-target flow chart。Use <see cref="VueUiFlowData.Link"/> for each positional row.</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-flow", "VueUiFlow")]
public sealed class VueUiFlow : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiFlowConfig>;

/// <summary>Galaxy chart，dataset 与 donut rows 共用。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-galaxy", "VueUiGalaxy")]
public sealed class VueUiGalaxy : VueDataUiChartComponent<VueUiDonutDatasetItem[], VueUiGalaxyConfig>;

/// <summary>地理点位 chart。上游允许 dataset omitted，因此不把参数错误标记为 required。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-geo", "VueUiGeo")]
public sealed class VueUiGeo : VueDataUiOptionalDatasetChartComponent<VueUiGeoDatasetItem[], VueUiGeoConfig>;

/// <summary>单值 gizmo visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-gizmo", "VueUiGizmo")]
public sealed class VueUiGizmo : VueDataUiChartComponent<double, VueUiGizmoConfig>;

/// <summary>可编辑 hill chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-hill", "VueUiHill")]
public sealed class VueUiHill : VueDataUiChartComponent<VueUiHillDatasetItem[], VueUiHillConfig>;

/// <summary>History plot chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-history-plot", "VueUiHistoryPlot")]
public sealed class VueUiHistoryPlot : VueDataUiChartComponent<VueUiHistoryPlotDatasetItem[], VueUiHistoryPlotConfig>;

/// <summary>vue-data-ui icon renderer。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-icon", "VueUiIcon")]
public sealed class VueUiIcon : ComponentBase
{
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VueUiIconName Name { get; set; }

    [Parameter]
    [ECMAScriptName("stroke")]
    public string? Stroke { get; set; }

    [Parameter]
    [ECMAScriptName("strokeWidth")]
    public double? StrokeWidth { get; set; }

    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    [Parameter]
    [ECMAScriptName("isSpin")]
    public bool? IsSpin { get; set; }

    [Parameter]
    [ECMAScriptName("spinDuration")]
    public string? SpinDuration { get; set; }
}

/// <summary>Mini loading indicator。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-mini-loader", "VueUiMiniLoader")]
public sealed class VueUiMiniLoader : VueDataUiConfigComponent<VueUiMiniLoaderConfig>;

/// <summary>递归 molecule graph。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-molecule", "VueUiMolecule")]
public sealed class VueUiMolecule : VueDataUiChartComponent<VueUiMoleculeDatasetNode[], VueUiMoleculeConfig>;

/// <summary>固定五档 mood radar。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-mood-radar", "VueUiMoodRadar")]
public sealed class VueUiMoodRadar : VueDataUiChartComponent<VueUiMoodRadarDataset, VueUiMoodRadarConfig>;

/// <summary>多环 nested donuts chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-nested-donuts", "VueUiNestedDonuts")]
public sealed class VueUiNestedDonuts : VueDataUiChartComponent<VueUiNestedDonutsDatasetItem[], VueUiNestedDonutsConfig>;

/// <summary>同心 onion chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-onion", "VueUiOnion")]
public sealed class VueUiOnion : VueDataUiChartComponent<VueUiOnionDatasetItem[], VueUiOnionConfig>;

/// <summary>平行坐标图。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-parallel-coordinate-plot", "VueUiParallelCoordinatePlot")]
public sealed class VueUiParallelCoordinatePlot : VueDataUiChartComponent<VueUiParallelCoordinatePlotDatasetItem[], VueUiParallelCoordinatePlotConfig>;

/// <summary>SVG pattern renderer。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-pattern", "VueUiPattern")]
public sealed class VueUiPattern : ComponentBase
{
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VueUiPatternName Name { get; set; }

    [Parameter]
    [EditorRequired]
    [ECMAScriptName("id")]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    [ECMAScriptName("fill")]
    public string? Fill { get; set; }

    [Parameter]
    [ECMAScriptName("stroke")]
    public string? Stroke { get; set; }

    [Parameter]
    [ECMAScriptName("strokeWidth")]
    public double? StrokeWidth { get; set; }

    [Parameter]
    [ECMAScriptName("scale")]
    public double? Scale { get; set; }
}

/// <summary>根据 seed 生成 deterministic SVG pattern。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-pattern-seed", "VueUiPatternSeed")]
public sealed class VueUiPatternSeed : ComponentBase
{
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("id")]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    [EditorRequired]
    [ECMAScriptName("seed")]
    public Vue.VueStringNumberValue Seed { get; set; } = default!;

    [Parameter]
    [ECMAScriptName("foregroundColor")]
    public string? ForegroundColor { get; set; }

    [Parameter]
    [ECMAScriptName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    [Parameter]
    [ECMAScriptName("maxSize")]
    public double? MaxSize { get; set; }

    [Parameter]
    [ECMAScriptName("minSize")]
    public double? MinSize { get; set; }

    [Parameter]
    [ECMAScriptName("disambiguator")]
    public Vue.VueStringNumberValue? Disambiguator { get; set; }
}

/// <summary>四象限 data visualization。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-quadrant", "VueUiQuadrant")]
public sealed class VueUiQuadrant : VueDataUiChartComponent<VueUiQuadrantDatasetItem[], VueUiQuadrantConfig>;

/// <summary>评分 visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-rating", "VueUiRating")]
public sealed class VueUiRating : VueDataUiChartComponent<VueUiRatingDataset, VueUiRatingConfig>;

/// <summary>关系网络圆图。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-relation-circle", "VueUiRelationCircle")]
public sealed class VueUiRelationCircle : VueDataUiChartComponent<VueUiRelationCircleDatasetItem[], VueUiRelationCircleConfig>;

/// <summary>多分布 ridgeline chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-ridgeline", "VueUiRidgeline")]
public sealed class VueUiRidgeline : VueDataUiChartComponent<VueUiRidgelineDatasetItem[], VueUiRidgelineConfig>;

/// <summary>多层 rings chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-rings", "VueUiRings")]
public sealed class VueUiRings : VueDataUiChartComponent<VueUiRingsDatasetItem[], VueUiRingsConfig>;

/// <summary>内置 chart skeleton renderer。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-skeleton", "VueUiSkeleton")]
public sealed class VueUiSkeleton : VueDataUiConfigComponent<VueUiSkeletonConfig>;

/// <summary>rating dataset 的 smiley presentation。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-smiley", "VueUiSmiley")]
public sealed class VueUiSmiley : VueDataUiChartComponent<VueUiRatingDataset, VueUiSmileyConfig>;

/// <summary>轻量 spark trend chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-spark-trend", "VueUiSparkTrend")]
public sealed class VueUiSparkTrend : VueDataUiChartComponent<double?[], VueUiSparkTrendConfig>;

/// <summary>单值 spark gauge。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-sparkgauge", "VueUiSparkgauge")]
public sealed class VueUiSparkgauge : VueDataUiChartComponent<VueUiSparkgaugeDataset, VueUiSparkgaugeConfig>;

/// <summary>轻量 stacked bar chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-sparkstackbar", "VueUiSparkStackbar")]
public sealed class VueUiSparkStackbar : VueDataUiChartComponent<VueUiSparkStackbarDatasetItem[], VueUiSparkStackbarConfig>;

/// <summary>Strip plot chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-strip-plot", "VueUiStripPlot")]
public sealed class VueUiStripPlot : VueDataUiChartComponent<VueUiStripPlotDataset[], VueUiStripPlotConfig>;

/// <summary>温度计 chart。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-thermometer", "VueUiThermometer")]
public sealed class VueUiThermometer : VueDataUiChartComponent<VueUiThermometerDataset, VueUiThermometerConfig>;

/// <summary>带 controls 的计时器 visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-timer", "VueUiTimer")]
public sealed class VueUiTimer : VueDataUiConfigComponent<VueUiTimerConfig>;

/// <summary>百分比 tiremarks visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-tiremarks", "VueUiTiremarks")]
public sealed class VueUiTiremarks : VueDataUiChartComponent<VueUiTiremarksDataset, VueUiTiremarksConfig>;

/// <summary>百分比 wheel visual。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-wheel", "VueUiWheel")]
public sealed class VueUiWheel : VueDataUiChartComponent<VueUiWheelDataset, VueUiWheelConfig>;

/// <summary>世界地图 chart。dataset is optional in the upstream prop contract.</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-world", "VueUiWorld")]
public sealed class VueUiWorld : VueDataUiOptionalDatasetChartComponent<VueUiWorldDataset, VueUiWorldConfig>;

/// <summary>Canvas renderer for large XY series。</summary>
[VueLibraryComponent("vue-data-ui/vue-ui-xy-canvas", "VueUiXyCanvas")]
public sealed class VueUiXyCanvas : VueDataUiChartComponent<VueUiXyCanvasDatasetItem[], VueUiXyCanvasConfig>;
