namespace ECMAScript.VueDataUi;

/// <summary>
/// vue-data-ui 3.23.4 完整公开 catalog 的其余 Razor descriptors。Each type imports its own
/// <c>vue-data-ui/vue-ui-*</c> entry so a component never pulls the aggregate root bundle.
/// </summary>
[ECMAScript("vue-data-ui/vue-ui-3d-bar", Transform.Component, "VueUi3dBar")]
public sealed class Vd3dBar : VdChartComponent<Vd3dBarDataset, Vd3dBarConfig>;

/// <summary>可折叠 content 容器。</summary>
[ECMAScript("vue-data-ui/vue-ui-accordion", Transform.Component, "VueUiAccordion")]
public sealed class VdAccordion : VdConfigComponent<VdAccordionConfig>
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
public sealed class VdAgePyramid : VdChartComponent<VdCellValue[][], VdAgePyramidConfig>;

/// <summary>图表 annotation overlay。</summary>
[ECMAScript("vue-data-ui/vue-ui-annotator", Transform.Component, "VueUiAnnotator")]
public sealed class VdAnnotator : VdOptionalDatasetChartComponent<VdAnnotatorDataset, VdAnnotatorConfig>;

/// <summary>排名变化的 bump chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-bump", Transform.Component, "VueUiBump")]
public sealed class VdBump : VdChartComponent<VdBumpDatasetItem[], VdBumpConfig>;

/// <summary>自动滚动 carousel table。</summary>
[ECMAScript("vue-data-ui/vue-ui-carousel-table", Transform.Component, "VueUiCarouselTable")]
public sealed class VdCarouselTable : VdChartComponent<VdCarouselTableDataset, VdCarouselTableConfig>;

/// <summary>层级 chestnut chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-chestnut", Transform.Component, "VueUiChestnut")]
public sealed class VdChestnut : VdChartComponent<VdChestnutDatasetRoot[], VdChestnutConfig>;

/// <summary>矩阵关系 chord chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-chord", Transform.Component, "VueUiChord")]
public sealed class VdChord : VdChartComponent<VdChordDataset, VdChordConfig>;

/// <summary>层级 circle pack chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-circle-pack", Transform.Component, "VueUiCirclePack")]
public sealed class VdCirclePack : VdChartComponent<VdCirclePackDatasetItem[], VdCirclePackConfig>;

/// <summary>交叉坐标 cursor overlay。</summary>
[ECMAScript("vue-data-ui/vue-ui-cursor", Transform.Component, "VueUiCursor")]
public sealed class VdCursor : VdConfigComponent<VdCursorConfig>;

/// <summary>有向无环图 visualization。</summary>
[ECMAScript("vue-data-ui/vue-ui-dag", Transform.Component, "VueUiDag")]
public sealed class VdDag : VdChartComponent<VdDagDataset, VdDagConfig>;

/// <summary>可拖动 chart dashboard layout。</summary>
[ECMAScript("vue-data-ui/vue-ui-dashboard", Transform.Component, "VueUiDashboard")]
public sealed class VdDashboard : VdChartComponent<VdDashboardElement[], VdDashboardConfig>
{
    /// <summary>
    /// 仪表板布局发生变化时触发；携带更新后的组件位置和尺寸。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onChange")]
    public EventCallback<VdDashboardPlacedElement[]> OnChange { get; set; }

    /// <summary>
    /// 复制仪表板的替代表示时触发；携带配置和当前布局数据。
    /// </summary>
    [Parameter]
    [ECMAScriptName("onCopyAlt")]
    public EventCallback<VdDashboardCopyAlt> OnCopyAlt { get; set; }
}

/// <summary>数码管数值 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-digits", Transform.Component, "VueUiDigits")]
public sealed class VdDigits : VdChartComponent<double, VdDigitsConfig>;

/// <summary>多时段 donut evolution chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-donut-evolution", Transform.Component, "VueUiDonutEvolution")]
public sealed class VdDonutEvolution : VdChartComponent<VdDonutEvolutionDatasetItem[], VdDonutEvolutionConfig>;

/// <summary>source-target flow chart。Use <see cref="VdFlowData.Link"/> for each positional row.</summary>
[ECMAScript("vue-data-ui/vue-ui-flow", Transform.Component, "VueUiFlow")]
public sealed class VdFlow : VdChartComponent<VdCellValue[][], VdFlowConfig>;

/// <summary>Galaxy chart，dataset 与 donut rows 共用。</summary>
[ECMAScript("vue-data-ui/vue-ui-galaxy", Transform.Component, "VueUiGalaxy")]
public sealed class VdGalaxy : VdChartComponent<VdDonutDatasetItem[], VdGalaxyConfig>;

/// <summary>地理点位 chart。上游允许 dataset omitted，因此不把参数错误标记为 required。</summary>
[ECMAScript("vue-data-ui/vue-ui-geo", Transform.Component, "VueUiGeo")]
public sealed class VdGeo : VdOptionalDatasetChartComponent<VdGeoDatasetItem[], VdGeoConfig>;

/// <summary>单值 gizmo visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-gizmo", Transform.Component, "VueUiGizmo")]
public sealed class VdGizmo : VdChartComponent<double, VdGizmoConfig>;

/// <summary>可编辑 hill chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-hill", Transform.Component, "VueUiHill")]
public sealed class VdHill : VdChartComponent<VdHillDatasetItem[], VdHillConfig>;

/// <summary>History plot chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-history-plot", Transform.Component, "VueUiHistoryPlot")]
public sealed class VdHistoryPlot : VdChartComponent<VdHistoryPlotDatasetItem[], VdHistoryPlotConfig>;

/// <summary>vue-data-ui icon renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-icon", Transform.Component, "VueUiIcon")]
public sealed class VdIcon : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// 要显示的内置图标；每个枚举成员说明对应的图形。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VdIconName Name { get; set; }

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
public sealed class VdMiniLoader : VdConfigComponent<VdMiniLoaderConfig>;

/// <summary>递归 molecule graph。</summary>
[ECMAScript("vue-data-ui/vue-ui-molecule", Transform.Component, "VueUiMolecule")]
public sealed class VdMolecule : VdChartComponent<VdMoleculeDatasetNode[], VdMoleculeConfig>;

/// <summary>固定五档 mood radar。</summary>
[ECMAScript("vue-data-ui/vue-ui-mood-radar", Transform.Component, "VueUiMoodRadar")]
public sealed class VdMoodRadar : VdChartComponent<VdMoodRadarDataset, VdMoodRadarConfig>;

/// <summary>多环 nested donuts chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-nested-donuts", Transform.Component, "VueUiNestedDonuts")]
public sealed class VdNestedDonuts : VdChartComponent<VdNestedDonutsDatasetItem[], VdNestedDonutsConfig>;

/// <summary>同心 onion chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-onion", Transform.Component, "VueUiOnion")]
public sealed class VdOnion : VdChartComponent<VdOnionDatasetItem[], VdOnionConfig>;

/// <summary>平行坐标图。</summary>
[ECMAScript("vue-data-ui/vue-ui-parallel-coordinate-plot", Transform.Component, "VueUiParallelCoordinatePlot")]
public sealed class VdParallelCoordinatePlot : VdChartComponent<VdParallelCoordinatePlotDatasetItem[], VdParallelCoordinatePlotConfig>;

/// <summary>SVG pattern renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-pattern", Transform.Component, "VueUiPattern")]
public sealed class VdPattern : ComponentBase
{
    /// <summary>
    /// 要使用的内置 SVG 填充图案。
    /// </summary>
    [Parameter]
    [EditorRequired]
    [ECMAScriptName("name")]
    public VdPatternName Name { get; set; }

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
public sealed class VdPatternSeed : ComponentBase
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
public sealed class VdQuadrant : VdChartComponent<VdQuadrantDatasetItem[], VdQuadrantConfig>;

/// <summary>评分 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-rating", Transform.Component, "VueUiRating")]
public sealed class VdRating : VdChartComponent<VdRatingDataset, VdRatingConfig>;

/// <summary>关系网络圆图。</summary>
[ECMAScript("vue-data-ui/vue-ui-relation-circle", Transform.Component, "VueUiRelationCircle")]
public sealed class VdRelationCircle : VdChartComponent<VdRelationCircleDatasetItem[], VdRelationCircleConfig>;

/// <summary>多分布 ridgeline chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-ridgeline", Transform.Component, "VueUiRidgeline")]
public sealed class VdRidgeline : VdChartComponent<VdRidgelineDatasetItem[], VdRidgelineConfig>;

/// <summary>多层 rings chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-rings", Transform.Component, "VueUiRings")]
public sealed class VdRings : VdChartComponent<VdRingsDatasetItem[], VdRingsConfig>;

/// <summary>内置 chart skeleton renderer。</summary>
[ECMAScript("vue-data-ui/vue-ui-skeleton", Transform.Component, "VueUiSkeleton")]
public sealed class VdSkeleton : VdConfigComponent<VdSkeletonConfig>;

/// <summary>rating dataset 的 smiley presentation。</summary>
[ECMAScript("vue-data-ui/vue-ui-smiley", Transform.Component, "VueUiSmiley")]
public sealed class VdSmiley : VdChartComponent<VdRatingDataset, VdSmileyConfig>;

/// <summary>轻量 spark trend chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-spark-trend", Transform.Component, "VueUiSparkTrend")]
public sealed class VdSparkTrend : VdChartComponent<double?[], VdSparkTrendConfig>;

/// <summary>单值 spark gauge。</summary>
[ECMAScript("vue-data-ui/vue-ui-sparkgauge", Transform.Component, "VueUiSparkgauge")]
public sealed class VdSparkgauge : VdChartComponent<VdSparkgaugeDataset, VdSparkgaugeConfig>;

/// <summary>轻量 stacked bar chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-sparkstackbar", Transform.Component, "VueUiSparkStackbar")]
public sealed class VdSparkStackbar : VdChartComponent<VdSparkStackbarDatasetItem[], VdSparkStackbarConfig>;

/// <summary>Strip plot chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-strip-plot", Transform.Component, "VueUiStripPlot")]
public sealed class VdStripPlot : VdChartComponent<VdStripPlotDataset[], VdStripPlotConfig>;

/// <summary>温度计 chart。</summary>
[ECMAScript("vue-data-ui/vue-ui-thermometer", Transform.Component, "VueUiThermometer")]
public sealed class VdThermometer : VdChartComponent<VdThermometerDataset, VdThermometerConfig>;

/// <summary>带 controls 的计时器 visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-timer", Transform.Component, "VueUiTimer")]
public sealed class VdTimer : VdConfigComponent<VdTimerConfig>;

/// <summary>百分比 tiremarks visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-tiremarks", Transform.Component, "VueUiTiremarks")]
public sealed class VdTiremarks : VdChartComponent<VdTiremarksDataset, VdTiremarksConfig>;

/// <summary>百分比 wheel visual。</summary>
[ECMAScript("vue-data-ui/vue-ui-wheel", Transform.Component, "VueUiWheel")]
public sealed class VdWheel : VdChartComponent<VdWheelDataset, VdWheelConfig>;

/// <summary>世界地图 chart。dataset is optional in the upstream prop contract.</summary>
[ECMAScript("vue-data-ui/vue-ui-world", Transform.Component, "VueUiWorld")]
public sealed class VdWorld : VdOptionalDatasetChartComponent<VdWorldDataset, VdWorldConfig>;

/// <summary>Canvas renderer for large XY series。</summary>
[ECMAScript("vue-data-ui/vue-ui-xy-canvas", Transform.Component, "VueUiXyCanvas")]
public sealed class VdXyCanvas : VdChartComponent<VdXyCanvasDatasetItem[], VdXyCanvasConfig>;