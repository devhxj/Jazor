namespace ECMAScript.VueDataUi;

/// <summary>
/// vue-data-ui 3.23.4 完整公开 catalog 的其余 Razor descriptors。Each type imports its own
/// <c>vue-data-ui/vue-ui-*</c> entry so a component never pulls the aggregate root bundle.
/// </summary>
[ECMAScriptName("VueUi3dBar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-3d-bar")]
public sealed class Vd3dBar : VdChartComponent<Vd3dBarDataset, Vd3dBarConfig>;

/// <summary>可折叠 content 容器。</summary>
[ECMAScriptName("VueUiAccordion")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-accordion")]
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
[ECMAScriptName("VueUiAgePyramid")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-age-pyramid")]
public sealed class VdAgePyramid : VdChartComponent<VdCellValue[][], VdAgePyramidConfig>;

/// <summary>图表 annotation overlay。</summary>
[ECMAScriptName("VueUiAnnotator")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-annotator")]
public sealed class VdAnnotator : VdOptionalDatasetChartComponent<VdAnnotatorDataset, VdAnnotatorConfig>;

/// <summary>排名变化的 bump chart。</summary>
[ECMAScriptName("VueUiBump")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-bump")]
public sealed class VdBump : VdChartComponent<VdBumpDatasetItem[], VdBumpConfig>;

/// <summary>自动滚动 carousel table。</summary>
[ECMAScriptName("VueUiCarouselTable")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-carousel-table")]
public sealed class VdCarouselTable : VdChartComponent<VdCarouselTableDataset, VdCarouselTableConfig>;

/// <summary>层级 chestnut chart。</summary>
[ECMAScriptName("VueUiChestnut")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-chestnut")]
public sealed class VdChestnut : VdChartComponent<VdChestnutDatasetRoot[], VdChestnutConfig>;

/// <summary>矩阵关系 chord chart。</summary>
[ECMAScriptName("VueUiChord")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-chord")]
public sealed class VdChord : VdChartComponent<VdChordDataset, VdChordConfig>;

/// <summary>层级 circle pack chart。</summary>
[ECMAScriptName("VueUiCirclePack")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-circle-pack")]
public sealed class VdCirclePack : VdChartComponent<VdCirclePackDatasetItem[], VdCirclePackConfig>;

/// <summary>交叉坐标 cursor overlay。</summary>
[ECMAScriptName("VueUiCursor")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-cursor")]
public sealed class VdCursor : VdConfigComponent<VdCursorConfig>;

/// <summary>有向无环图 visualization。</summary>
[ECMAScriptName("VueUiDag")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-dag")]
public sealed class VdDag : VdChartComponent<VdDagDataset, VdDagConfig>;

/// <summary>可拖动 chart dashboard layout。</summary>
[ECMAScriptName("VueUiDashboard")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-dashboard")]
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
[ECMAScriptName("VueUiDigits")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-digits")]
public sealed class VdDigits : VdChartComponent<double, VdDigitsConfig>;

/// <summary>多时段 donut evolution chart。</summary>
[ECMAScriptName("VueUiDonutEvolution")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-donut-evolution")]
public sealed class VdDonutEvolution : VdChartComponent<VdDonutEvolutionDatasetItem[], VdDonutEvolutionConfig>;

/// <summary>source-target flow chart。Use <see cref="VdFlowData.Link"/> for each positional row.</summary>
[ECMAScriptName("VueUiFlow")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-flow")]
public sealed class VdFlow : VdChartComponent<VdCellValue[][], VdFlowConfig>;

/// <summary>Galaxy chart，dataset 与 donut rows 共用。</summary>
[ECMAScriptName("VueUiGalaxy")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-galaxy")]
public sealed class VdGalaxy : VdChartComponent<VdDonutDatasetItem[], VdGalaxyConfig>;

/// <summary>地理点位 chart。上游允许 dataset omitted，因此不把参数错误标记为 required。</summary>
[ECMAScriptName("VueUiGeo")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-geo")]
public sealed class VdGeo : VdOptionalDatasetChartComponent<VdGeoDatasetItem[], VdGeoConfig>;

/// <summary>单值 gizmo visual。</summary>
[ECMAScriptName("VueUiGizmo")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-gizmo")]
public sealed class VdGizmo : VdChartComponent<double, VdGizmoConfig>;

/// <summary>可编辑 hill chart。</summary>
[ECMAScriptName("VueUiHill")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-hill")]
public sealed class VdHill : VdChartComponent<VdHillDatasetItem[], VdHillConfig>;

/// <summary>History plot chart。</summary>
[ECMAScriptName("VueUiHistoryPlot")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-history-plot")]
public sealed class VdHistoryPlot : VdChartComponent<VdHistoryPlotDatasetItem[], VdHistoryPlotConfig>;

/// <summary>vue-data-ui icon renderer。</summary>
[ECMAScriptName("VueUiIcon")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-icon")]
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
[ECMAScriptName("VueUiMiniLoader")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-mini-loader")]
public sealed class VdMiniLoader : VdConfigComponent<VdMiniLoaderConfig>;

/// <summary>递归 molecule graph。</summary>
[ECMAScriptName("VueUiMolecule")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-molecule")]
public sealed class VdMolecule : VdChartComponent<VdMoleculeDatasetNode[], VdMoleculeConfig>;

/// <summary>固定五档 mood radar。</summary>
[ECMAScriptName("VueUiMoodRadar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-mood-radar")]
public sealed class VdMoodRadar : VdChartComponent<VdMoodRadarDataset, VdMoodRadarConfig>;

/// <summary>多环 nested donuts chart。</summary>
[ECMAScriptName("VueUiNestedDonuts")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-nested-donuts")]
public sealed class VdNestedDonuts : VdChartComponent<VdNestedDonutsDatasetItem[], VdNestedDonutsConfig>;

/// <summary>同心 onion chart。</summary>
[ECMAScriptName("VueUiOnion")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-onion")]
public sealed class VdOnion : VdChartComponent<VdOnionDatasetItem[], VdOnionConfig>;

/// <summary>平行坐标图。</summary>
[ECMAScriptName("VueUiParallelCoordinatePlot")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-parallel-coordinate-plot")]
public sealed class VdParallelCoordinatePlot : VdChartComponent<VdParallelCoordinatePlotDatasetItem[], VdParallelCoordinatePlotConfig>;

/// <summary>SVG pattern renderer。</summary>
[ECMAScriptName("VueUiPattern")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-pattern")]
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
[ECMAScriptName("VueUiPatternSeed")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-pattern-seed")]
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
[ECMAScriptName("VueUiQuadrant")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-quadrant")]
public sealed class VdQuadrant : VdChartComponent<VdQuadrantDatasetItem[], VdQuadrantConfig>;

/// <summary>评分 visual。</summary>
[ECMAScriptName("VueUiRating")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-rating")]
public sealed class VdRating : VdChartComponent<VdRatingDataset, VdRatingConfig>;

/// <summary>关系网络圆图。</summary>
[ECMAScriptName("VueUiRelationCircle")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-relation-circle")]
public sealed class VdRelationCircle : VdChartComponent<VdRelationCircleDatasetItem[], VdRelationCircleConfig>;

/// <summary>多分布 ridgeline chart。</summary>
[ECMAScriptName("VueUiRidgeline")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-ridgeline")]
public sealed class VdRidgeline : VdChartComponent<VdRidgelineDatasetItem[], VdRidgelineConfig>;

/// <summary>多层 rings chart。</summary>
[ECMAScriptName("VueUiRings")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-rings")]
public sealed class VdRings : VdChartComponent<VdRingsDatasetItem[], VdRingsConfig>;

/// <summary>内置 chart skeleton renderer。</summary>
[ECMAScriptName("VueUiSkeleton")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-skeleton")]
public sealed class VdSkeleton : VdConfigComponent<VdSkeletonConfig>;

/// <summary>rating dataset 的 smiley presentation。</summary>
[ECMAScriptName("VueUiSmiley")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-smiley")]
public sealed class VdSmiley : VdChartComponent<VdRatingDataset, VdSmileyConfig>;

/// <summary>轻量 spark trend chart。</summary>
[ECMAScriptName("VueUiSparkTrend")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-spark-trend")]
public sealed class VdSparkTrend : VdChartComponent<double?[], VdSparkTrendConfig>;

/// <summary>单值 spark gauge。</summary>
[ECMAScriptName("VueUiSparkgauge")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-sparkgauge")]
public sealed class VdSparkgauge : VdChartComponent<VdSparkgaugeDataset, VdSparkgaugeConfig>;

/// <summary>轻量 stacked bar chart。</summary>
[ECMAScriptName("VueUiSparkStackbar")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-sparkstackbar")]
public sealed class VdSparkStackbar : VdChartComponent<VdSparkStackbarDatasetItem[], VdSparkStackbarConfig>;

/// <summary>Strip plot chart。</summary>
[ECMAScriptName("VueUiStripPlot")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-strip-plot")]
public sealed class VdStripPlot : VdChartComponent<VdStripPlotDataset[], VdStripPlotConfig>;

/// <summary>温度计 chart。</summary>
[ECMAScriptName("VueUiThermometer")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-thermometer")]
public sealed class VdThermometer : VdChartComponent<VdThermometerDataset, VdThermometerConfig>;

/// <summary>带 controls 的计时器 visual。</summary>
[ECMAScriptName("VueUiTimer")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-timer")]
public sealed class VdTimer : VdConfigComponent<VdTimerConfig>;

/// <summary>百分比 tiremarks visual。</summary>
[ECMAScriptName("VueUiTiremarks")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-tiremarks")]
public sealed class VdTiremarks : VdChartComponent<VdTiremarksDataset, VdTiremarksConfig>;

/// <summary>百分比 wheel visual。</summary>
[ECMAScriptName("VueUiWheel")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-wheel")]
public sealed class VdWheel : VdChartComponent<VdWheelDataset, VdWheelConfig>;

/// <summary>世界地图 chart。dataset is optional in the upstream prop contract.</summary>
[ECMAScriptName("VueUiWorld")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-world")]
public sealed class VdWorld : VdOptionalDatasetChartComponent<VdWorldDataset, VdWorldConfig>;

/// <summary>Canvas renderer for large XY series。</summary>
[ECMAScriptName("VueUiXyCanvas")]
[Style("vue-data-ui/style.css")]
[ECMAScript("vue-data-ui/vue-ui-xy-canvas")]
public sealed class VdXyCanvas : VdChartComponent<VdXyCanvasDatasetItem[], VdXyCanvasConfig>;
