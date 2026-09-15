namespace ECMAScript.VueDataUi;

/// <summary>
/// vue-data-ui 3.23.4 完整公开 catalog 的其余 Razor descriptors。Each type imports its own
/// <c>vue-data-ui/vue-ui-*</c> entry so a component never pulls the aggregate root bundle.
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-3d-bar", Transform.Component, "VueUi3dBar")]
public sealed class VueUi3dBar : VueDataUiChartComponent<VueUi3dBarDataset, VueUi3dBarConfig>;

/// <summary>可折叠 content 容器。</summary>
[ECMAScript("vue-data-ui/vue-ui-accordion", Transform.Component, "VueUiAccordion")]
public sealed class VueUiAccordion : VueDataUiConfigComponent<VueUiAccordionConfig>
{
    /// <summary>
    /// 隐藏手风琴的详细内容区域。
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public bool? HideDetails { get; set; }
}

/// <summary>年龄金字塔图表。</summary>
[ECMAScript("vue-data-ui/vue-ui-age-pyramid", Transform.Component, "VueUiAgePyramid")]
public sealed class VueUiAgePyramid : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiAgePyramidConfig>;

/// <summary>图表 annotation overlay。</summary>
[ECMAScript("vue-data-ui/vue-ui-annotator", Transform.Component, "VueUiAnnotator")]
public sealed class VueUiAnnotator : VueDataUiOptionalDatasetChartComponent<VueUiAnnotatorDataset, VueUiAnnotatorConfig>;

/// <summary>排名变化的 bump chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-bump", Transform.Component, "VueUiBump")]
public sealed class VueUiBump : VueDataUiChartComponent<VueUiBumpDatasetItem[], VueUiBumpConfig>;

/// <summary>自动滚动 carousel table。</summary>
[ECMAScript("vue-data-ui/vue-ui-carousel-table", Transform.Component, "VueUiCarouselTable")]
public sealed class VueUiCarouselTable : VueDataUiChartComponent<VueUiCarouselTableDataset, VueUiCarouselTableConfig>;

/// <summary>层级 chestnut chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-chestnut", Transform.Component, "VueUiChestnut")]
public sealed class VueUiChestnut : VueDataUiChartComponent<VueUiChestnutDatasetRoot[], VueUiChestnutConfig>;

/// <summary>矩阵关系 chord chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-chord", Transform.Component, "VueUiChord")]
public sealed class VueUiChord : VueDataUiChartComponent<VueUiChordDataset, VueUiChordConfig>;

/// <summary>层级 circle pack chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-circle-pack", Transform.Component, "VueUiCirclePack")]
public sealed class VueUiCirclePack : VueDataUiChartComponent<VueUiCirclePackDatasetItem[], VueUiCirclePackConfig>;

/// <summary>交叉坐标 cursor overlay。</summary>
[ECMAScript("vue-data-ui/vue-ui-cursor", Transform.Component, "VueUiCursor")]
public sealed class VueUiCursor : VueDataUiConfigComponent<VueUiCursorConfig>;

/// <summary>有向无环图 visualization。</summary>
[ECMAScript("vue-data-ui/vue-ui-dag", Transform.Component, "VueUiDag")]
public sealed class VueUiDag : VueDataUiChartComponent<VueUiDagDataset, VueUiDagConfig>;

/// <summary>可拖动 chart dashboard layout。</summary>
[ECMAScript("vue-data-ui/vue-ui-dashboard", Transform.Component, "VueUiDashboard")]
public sealed class VueUiDashboard : VueDataUiChartComponent<VueUiDashboardElement[], VueUiDashboardConfig>
{
    /// <summary>
    /// 仪表板布局发生变化时触发；携带更新后的组件位置和尺寸。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onChange")]
    public EventCallback<VueUiDashboardPlacedElement[]> OnChange { get; set; }

    /// <summary>
    /// 复制仪表板的替代表示时触发；携带配置和当前布局数据。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onCopyAlt")]
    public EventCallback<VueUiDashboardCopyAlt> OnCopyAlt { get; set; }
}

/// <summary>数码管数值 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-digits", Transform.Component, "VueUiDigits")]
public sealed class VueUiDigits : VueDataUiChartComponent<double, VueUiDigitsConfig>;

/// <summary>多时段 donut evolution chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-donut-evolution", Transform.Component, "VueUiDonutEvolution")]
public sealed class VueUiDonutEvolution : VueDataUiChartComponent<VueUiDonutEvolutionDatasetItem[], VueUiDonutEvolutionConfig>;

/// <summary>source-target flow chart。Use <see cref="VueUiFlowData.Link"/> for each positional row.</summary>
[ECMAScript("vue-data-ui/vue-ui-flow", Transform.Component, "VueUiFlow")]
public sealed class VueUiFlow : VueDataUiChartComponent<VueDataUiCellValue[][], VueUiFlowConfig>;

/// <summary>Galaxy chart，dataset 与 donut rows 共用。</summary>
[ECMAScript("vue-data-ui/vue-ui-galaxy", Transform.Component, "VueUiGalaxy")]
public sealed class VueUiGalaxy : VueDataUiChartComponent<VueUiDonutDatasetItem[], VueUiGalaxyConfig>;

/// <summary>地理点位 chart。上游允许 dataset omitted，因此不把参数错误标记为 required。</summary>
[ECMAScript("vue-data-ui/vue-ui-geo", Transform.Component, "VueUiGeo")]
public sealed class VueUiGeo : VueDataUiOptionalDatasetChartComponent<VueUiGeoDatasetItem[], VueUiGeoConfig>;

/// <summary>单值 gizmo visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-gizmo", Transform.Component, "VueUiGizmo")]
public sealed class VueUiGizmo : VueDataUiChartComponent<double, VueUiGizmoConfig>;

/// <summary>可编辑 hill chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-hill", Transform.Component, "VueUiHill")]
public sealed class VueUiHill : VueDataUiChartComponent<VueUiHillDatasetItem[], VueUiHillConfig>;

/// <summary>History plot chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-history-plot", Transform.Component, "VueUiHistoryPlot")]
public sealed class VueUiHistoryPlot : VueDataUiChartComponent<VueUiHistoryPlotDatasetItem[], VueUiHistoryPlotConfig>;

/// <summary>vue-data-ui icon renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-icon", Transform.Component, "VueUiIcon")]
public sealed class VueUiIcon : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// 要显示的内置图标；每个枚举成员说明对应的图形。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VueUiIconName Name { get; set; }

    /// <summary>
    /// SVG 轮廓使用的描边颜色。
    /// </summary>
    [Parameter]
    [ECMAScriptName("stroke")]
    public string? Stroke { get; set; }

    /// <summary>
    /// SVG 描边线宽，以 SVG 用户坐标单位表示。
    /// </summary>
    [Parameter]
    [ECMAScriptName("strokeWidth")]
    public double? StrokeWidth { get; set; }

    /// <summary>
    /// 图标显示尺寸，可使用数值或带单位的 CSS 尺寸。
    /// </summary>
    [Parameter]
    [ECMAScriptName("size")]
    public Vue.VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 为图标启用持续旋转动画。
    /// </summary>
    [Parameter]
    [ECMAScriptName("isSpin")]
    public bool? IsSpin { get; set; }

    /// <summary>
    /// 图标完成一周旋转的 CSS 时间，例如 1s。
    /// </summary>
    [Parameter]
    [ECMAScriptName("spinDuration")]
    public string? SpinDuration { get; set; }
}

/// <summary>Mini loading indicator。</summary>
[ECMAScript("vue-data-ui/vue-ui-mini-loader", Transform.Component, "VueUiMiniLoader")]
public sealed class VueUiMiniLoader : VueDataUiConfigComponent<VueUiMiniLoaderConfig>;

/// <summary>递归 molecule graph。</summary>
[ECMAScript("vue-data-ui/vue-ui-molecule", Transform.Component, "VueUiMolecule")]
public sealed class VueUiMolecule : VueDataUiChartComponent<VueUiMoleculeDatasetNode[], VueUiMoleculeConfig>;

/// <summary>固定五档 mood radar。</summary>
[ECMAScript("vue-data-ui/vue-ui-mood-radar", Transform.Component, "VueUiMoodRadar")]
public sealed class VueUiMoodRadar : VueDataUiChartComponent<VueUiMoodRadarDataset, VueUiMoodRadarConfig>;

/// <summary>多环 nested donuts chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-nested-donuts", Transform.Component, "VueUiNestedDonuts")]
public sealed class VueUiNestedDonuts : VueDataUiChartComponent<VueUiNestedDonutsDatasetItem[], VueUiNestedDonutsConfig>;

/// <summary>同心 onion chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-onion", Transform.Component, "VueUiOnion")]
public sealed class VueUiOnion : VueDataUiChartComponent<VueUiOnionDatasetItem[], VueUiOnionConfig>;

/// <summary>平行坐标图。</summary>
[ECMAScript("vue-data-ui/vue-ui-parallel-coordinate-plot", Transform.Component, "VueUiParallelCoordinatePlot")]
public sealed class VueUiParallelCoordinatePlot : VueDataUiChartComponent<VueUiParallelCoordinatePlotDatasetItem[], VueUiParallelCoordinatePlotConfig>;

/// <summary>SVG pattern renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-pattern", Transform.Component, "VueUiPattern")]
public sealed class VueUiPattern : ComponentBase
{
    /// <summary>
    /// 要使用的内置 SVG 填充图案。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VueUiPatternName Name { get; set; }

    /// <summary>
    /// 当前 SVG 图案的唯一 id；使用 url(#id) 引用填充时应保持一致。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// SVG 图案的填充颜色。
    /// </summary>
    [Parameter]
    [ECMAScriptName("fill")]
    public string? Fill { get; set; }

    /// <summary>
    /// SVG 轮廓使用的描边颜色。
    /// </summary>
    [Parameter]
    [ECMAScriptName("stroke")]
    public string? Stroke { get; set; }

    /// <summary>
    /// SVG 描边线宽，以 SVG 用户坐标单位表示。
    /// </summary>
    [Parameter]
    [ECMAScriptName("strokeWidth")]
    public double? StrokeWidth { get; set; }

    /// <summary>
    /// 缩放 SVG 图案的比例。
    /// </summary>
    [Parameter]
    [ECMAScriptName("scale")]
    public double? Scale { get; set; }
}

/// <summary>根据 seed 生成 deterministic SVG pattern。</summary>
[ECMAScript("vue-data-ui/vue-ui-pattern-seed", Transform.Component, "VueUiPatternSeed")]
public sealed class VueUiPatternSeed : ComponentBase
{
    /// <summary>
    /// 当前 SVG 图案的唯一 id；使用 url(#id) 引用填充时应保持一致。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 确定性图案的随机种子；相同种子和配置生成相同的图案。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("seed")]
    public Vue.VueStringNumberValue Seed { get; set; } = default!;

    /// <summary>
    /// 图案前景的 CSS 颜色值。
    /// </summary>
    [Parameter]
    [ECMAScriptName("foregroundColor")]
    public string? ForegroundColor { get; set; }

    /// <summary>
    /// 背景的 CSS 颜色值。
    /// </summary>
    [Parameter]
    [ECMAScriptName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// 种子图案允许的最大图形尺寸。
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxSize")]
    public double? MaxSize { get; set; }

    /// <summary>
    /// 种子图案允许的最小图形尺寸。
    /// </summary>
    [Parameter]
    [ECMAScriptName("minSize")]
    public double? MinSize { get; set; }

    /// <summary>
    /// 区分使用相同种子的多个图案实例，避免 SVG 标识冲突。
    /// </summary>
    [Parameter]
    [ECMAScriptName("disambiguator")]
    public Vue.VueStringNumberValue? Disambiguator { get; set; }
}

/// <summary>四象限 data visualization。</summary>
[ECMAScript("vue-data-ui/vue-ui-quadrant", Transform.Component, "VueUiQuadrant")]
public sealed class VueUiQuadrant : VueDataUiChartComponent<VueUiQuadrantDatasetItem[], VueUiQuadrantConfig>;

/// <summary>评分 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-rating", Transform.Component, "VueUiRating")]
public sealed class VueUiRating : VueDataUiChartComponent<VueUiRatingDataset, VueUiRatingConfig>;

/// <summary>关系网络圆图。</summary>
[ECMAScript("vue-data-ui/vue-ui-relation-circle", Transform.Component, "VueUiRelationCircle")]
public sealed class VueUiRelationCircle : VueDataUiChartComponent<VueUiRelationCircleDatasetItem[], VueUiRelationCircleConfig>;

/// <summary>多分布 ridgeline chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-ridgeline", Transform.Component, "VueUiRidgeline")]
public sealed class VueUiRidgeline : VueDataUiChartComponent<VueUiRidgelineDatasetItem[], VueUiRidgelineConfig>;

/// <summary>多层 rings chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-rings", Transform.Component, "VueUiRings")]
public sealed class VueUiRings : VueDataUiChartComponent<VueUiRingsDatasetItem[], VueUiRingsConfig>;

/// <summary>内置 chart skeleton renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-skeleton", Transform.Component, "VueUiSkeleton")]
public sealed class VueUiSkeleton : VueDataUiConfigComponent<VueUiSkeletonConfig>;

/// <summary>rating dataset 的 smiley presentation。</summary>
[ECMAScript("vue-data-ui/vue-ui-smiley", Transform.Component, "VueUiSmiley")]
public sealed class VueUiSmiley : VueDataUiChartComponent<VueUiRatingDataset, VueUiSmileyConfig>;

/// <summary>轻量 spark trend chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-spark-trend", Transform.Component, "VueUiSparkTrend")]
public sealed class VueUiSparkTrend : VueDataUiChartComponent<double?[], VueUiSparkTrendConfig>;

/// <summary>单值 spark gauge。</summary>
[ECMAScript("vue-data-ui/vue-ui-sparkgauge", Transform.Component, "VueUiSparkgauge")]
public sealed class VueUiSparkgauge : VueDataUiChartComponent<VueUiSparkgaugeDataset, VueUiSparkgaugeConfig>;

/// <summary>轻量 stacked bar chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-sparkstackbar", Transform.Component, "VueUiSparkStackbar")]
public sealed class VueUiSparkStackbar : VueDataUiChartComponent<VueUiSparkStackbarDatasetItem[], VueUiSparkStackbarConfig>;

/// <summary>Strip plot chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-strip-plot", Transform.Component, "VueUiStripPlot")]
public sealed class VueUiStripPlot : VueDataUiChartComponent<VueUiStripPlotDataset[], VueUiStripPlotConfig>;

/// <summary>温度计 chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-thermometer", Transform.Component, "VueUiThermometer")]
public sealed class VueUiThermometer : VueDataUiChartComponent<VueUiThermometerDataset, VueUiThermometerConfig>;

/// <summary>带 controls 的计时器 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-timer", Transform.Component, "VueUiTimer")]
public sealed class VueUiTimer : VueDataUiConfigComponent<VueUiTimerConfig>;

/// <summary>百分比 tiremarks visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-tiremarks", Transform.Component, "VueUiTiremarks")]
public sealed class VueUiTiremarks : VueDataUiChartComponent<VueUiTiremarksDataset, VueUiTiremarksConfig>;

/// <summary>百分比 wheel visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-wheel", Transform.Component, "VueUiWheel")]
public sealed class VueUiWheel : VueDataUiChartComponent<VueUiWheelDataset, VueUiWheelConfig>;

/// <summary>世界地图 chart。dataset is optional in the upstream prop contract.</summary>
[ECMAScript("vue-data-ui/vue-ui-world", Transform.Component, "VueUiWorld")]
public sealed class VueUiWorld : VueDataUiOptionalDatasetChartComponent<VueUiWorldDataset, VueUiWorldConfig>;

/// <summary>Canvas renderer for large XY series。</summary>
[ECMAScript("vue-data-ui/vue-ui-xy-canvas", Transform.Component, "VueUiXyCanvas")]
public sealed class VueUiXyCanvas : VueDataUiChartComponent<VueUiXyCanvasDatasetItem[], VueUiXyCanvasConfig>;