namespace ECMAScript.VueDataUi;

// 3.23.4 complete catalog: these records intentionally model each component's stable input shape.
// Deep style/options remain extensible through VueDataUiConfig/VueDataUiDatasetItem, never object.

/// <summary>VueUi3dBar 的 breakdown 条目。</summary>
[ECMAScript]
[Description("@#")]
public record VueUi3dBarDatasetBreakdown : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }
}

/// <summary>VueUi3dBar 的单个 series。</summary>
[ECMAScript]
[Description("@#")]
public record VueUi3dBarDatasetSeriesItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#breakdown")]
    public VueUi3dBarDatasetBreakdown[]? Breakdown { get; init; }
}

/// <summary>VueUi3dBar dataset 根对象。</summary>
[ECMAScript]
[Description("@#")]
public record VueUi3dBarDataset : Vue.VueProps
{
    [Description("@#percentage")]
    public double? Percentage { get; init; }

    [Description("@#series")]
    public VueUi3dBarDatasetSeriesItem[]? Series { get; init; }
}

/// <summary>3D bar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUi3dBarConfig : VueDataUiConfig;

/// <summary>Accordion configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiAccordionConfig : VueDataUiConfig;

/// <summary>Age pyramid configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiAgePyramidConfig : VueDataUiConfig;

/// <summary>
/// Age pyramid positional row helper。C# tuple 会 lower 为 object，所以此 helper owns the array shape
/// required by upstream: <c>[year, rank, left, right]</c>。
/// </summary>
public static class VueUiAgePyramidData
{
    [ECMAScriptInline("[__arg1, __arg2, __arg3, __arg4]")]
    public extern static VueDataUiCellValue[] Row(string year, double rank, double? left, double? right);
}

/// <summary>Annotator 可选 dataset 的结构化 object。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiAnnotatorDataset : VueDataUiDatasetItem;

/// <summary>Annotator configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiAnnotatorConfig : VueDataUiConfig;

/// <summary>Bump chart 的 series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBumpDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Bump chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBumpConfig : VueDataUiConfig;

/// <summary>Carousel table dataset。每行 cell 保持 string/number closed domain。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCarouselTableDataset : Vue.VueProps
{
    [Description("@#head")]
    public string[] Head { get; init; } = [];

    [Description("@#body")]
    public VueDataUiCellValue[][] Body { get; init; } = [];
}

/// <summary>Carousel table configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCarouselTableConfig : VueDataUiConfig;

/// <summary>Chestnut chart breakdown leaf。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChestnutDatasetBranchBreakdown : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Chestnut chart branch。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChestnutDatasetBranch : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#breakdown")]
    public VueUiChestnutDatasetBranchBreakdown[]? Breakdown { get; init; }
}

/// <summary>Chestnut chart root node。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChestnutDatasetRoot : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#branches")]
    public VueUiChestnutDatasetBranch[] Branches { get; init; } = [];
}

/// <summary>Chestnut chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChestnutConfig : VueDataUiConfig;

/// <summary>Chord matrix dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChordDataset : Vue.VueProps
{
    [Description("@#matrix")]
    public double?[][] Matrix { get; init; } = [];

    [Description("@#labels")]
    public string[]? Labels { get; init; }

    [Description("@#colors")]
    public string[]? Colors { get; init; }
}

/// <summary>Chord chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiChordConfig : VueDataUiConfig;

/// <summary>Circle pack hierarchy node。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCirclePackDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#children")]
    public VueUiCirclePackDatasetItem[]? Children { get; init; }
}

/// <summary>Circle pack configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCirclePackConfig : VueDataUiConfig;

/// <summary>Cursor visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCursorConfig : VueDataUiConfig;

/// <summary>DAG node authored by callers。额外 metadata 可通过继承的 dictionary 传递。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDagNode : VueDataUiDatasetItem
{
    [Description("@#id")]
    public string Id { get; init; } = string.Empty;

    [Description("@#label")]
    public string Label { get; init; } = string.Empty;

    [Description("@#backgroundColor")]
    public string? BackgroundColor { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>DAG edge authored by callers。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDagEdge : Vue.VueProps
{
    [Description("@#from")]
    public string From { get; init; } = string.Empty;

    [Description("@#to")]
    public string To { get; init; } = string.Empty;

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#animated")]
    public bool? Animated { get; init; }

    [Description("@#dasharray")]
    public string? Dasharray { get; init; }

    [Description("@#animationDurationMs")]
    public double? AnimationDurationMs { get; init; }

    [Description("@#animationDirection")]
    public double? AnimationDirection { get; init; }
}

/// <summary>DAG dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDagDataset : Vue.VueProps
{
    [Description("@#nodes")]
    public VueUiDagNode[] Nodes { get; init; } = [];

    [Description("@#edges")]
    public VueUiDagEdge[] Edges { get; init; } = [];
}

/// <summary>DAG chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDagConfig : VueDataUiConfig;

/// <summary>
/// Dashboard item props base。具体 chart 可使用 <see cref="VueUiDashboardElementProps{TDataset,TConfig}"/>
/// 保持内部 dataset/config 的 exact C# type。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardElementProps : Vue.VueProps;

/// <summary>Dashboard 中一个具体图表的 typed props。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardElementProps<TDataset, TConfig> : VueUiDashboardElementProps
{
    [Description("@#dataset")]
    public TDataset Dataset { get; init; } = default!;

    [Description("@#config")]
    public TConfig? Config { get; init; }
}

/// <summary>Dashboard grid 中的一个 component placement。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardElement : VueDataUiDatasetItem
{
    [Description("@#id")]
    public Vue.VueStringNumberValue Id { get; init; } = default!;

    [Description("@#width")]
    public double Width { get; init; }

    [Description("@#height")]
    public double Height { get; init; }

    [Description("@#left")]
    public double Left { get; init; }

    [Description("@#top")]
    public double Top { get; init; }

    [Description("@#component")]
    public string Component { get; init; } = string.Empty;

    [Description("@#props")]
    public VueUiDashboardElementProps? Props { get; init; }
}

/// <summary>Dashboard <c>change</c> event 的 placement shape。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardPlacedElement : Vue.VueProps
{
    [Description("@#component")]
    public string Component { get; init; } = string.Empty;

    [Description("@#height")]
    public double Height { get; init; }

    [Description("@#id")]
    public string Id { get; init; } = string.Empty;

    [Description("@#index")]
    public double Index { get; init; }

    [Description("@#left")]
    public double Left { get; init; }

    [Description("@#top")]
    public double Top { get; init; }

    [Description("@#width")]
    public double Width { get; init; }
}

/// <summary>Dashboard <c>copyAlt</c> event payload。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardCopyAlt : Vue.VueProps
{
    [Description("@#config")]
    public VueUiDashboardConfig Config { get; init; } = default!;

    [Description("@#dataset")]
    public VueUiDashboardPlacedElement[] Dataset { get; init; } = [];
}

/// <summary>Dashboard configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDashboardConfig : VueDataUiConfig;

/// <summary>Digits visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDigitsConfig : VueDataUiConfig
{
    [Description("@#backgroundColor")]
    public string? BackgroundColor { get; init; }

    [Description("@#height")]
    public string? Height { get; init; }

    [Description("@#width")]
    public string? Width { get; init; }
}

/// <summary>Donut evolution 的 series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutEvolutionDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Donut evolution configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutEvolutionConfig : VueDataUiConfig;

/// <summary>
/// Flow link array helper。The upstream tuple must remain a JavaScript array rather than a C# tuple object.
/// </summary>
public static class VueUiFlowData
{
    [ECMAScriptInline("[__arg1, __arg2, __arg3]")]
    public extern static VueDataUiCellValue[] Link(string from, string to, double? value);
}

/// <summary>Flow chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiFlowConfig : VueDataUiConfig;

/// <summary>Galaxy configuration。Dataset reuses <see cref="VueUiDonutDatasetItem"/> rows.</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGalaxyConfig : VueDataUiConfig;

/// <summary>Geo map point。Coordinates are emitted as a two-item JavaScript array.</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGeoDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#coordinates")]
    public double[] Coordinates { get; init; } = [];

    [Description("@#description")]
    public string? Description { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#radius")]
    public double? Radius { get; init; }
}

/// <summary>Geo map configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGeoConfig : VueDataUiConfig;

/// <summary>Gizmo configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGizmoConfig : VueDataUiConfig;

/// <summary>Hill chart item。The chart deliberately accepts arbitrary metadata alongside these stable fields.</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHillDatasetItem : VueDataUiDatasetItem
{
    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#muted")]
    public bool? Muted { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>Hill chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHillConfig : VueDataUiConfig;

/// <summary>History plot coordinate。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHistoryPlotValue : Vue.VueProps
{
    [Description("@#x")]
    public double X { get; init; }

    [Description("@#y")]
    public double Y { get; init; }

    [Description("@#label")]
    public string? Label { get; init; }
}

/// <summary>History plot series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHistoryPlotDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public VueUiHistoryPlotValue[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#smooth")]
    public bool? Smooth { get; init; }

    [Description("@#temperatureColors")]
    public string[]? TemperatureColors { get; init; }

    [Description("@#temperatureAngle")]
    public double? TemperatureAngle { get; init; }

    [Description("@#usePlotTemperatureColors")]
    public bool? UsePlotTemperatureColors { get; init; }

    [Description("@#temperatureIndependant")]
    public bool? TemperatureIndependant { get; init; }
}

/// <summary>History plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHistoryPlotConfig : VueDataUiConfig;

/// <summary>Mini loader configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiMiniLoaderConfig : VueDataUiConfig;

/// <summary>Molecule graph node。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiMoleculeDatasetNode : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#details")]
    public string? Details { get; init; }

    [Description("@#nodes")]
    public VueUiMoleculeDatasetNode[]? Nodes { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Molecule graph configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiMoleculeConfig : VueDataUiConfig;

/// <summary>Mood radar's fixed five score buckets。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiMoodRadarDataset : Vue.VueProps
{
    [Description("@#1")]
    public double One { get; init; }

    [Description("@#2")]
    public double Two { get; init; }

    [Description("@#3")]
    public double Three { get; init; }

    [Description("@#4")]
    public double Four { get; init; }

    [Description("@#5")]
    public double Five { get; init; }
}

/// <summary>Mood radar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiMoodRadarConfig : VueDataUiConfig;

/// <summary>Nested donut ring and its inner donut series。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiNestedDonutsDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#series")]
    public VueUiDonutDatasetItem[] Series { get; init; } = [];
}

/// <summary>Nested donuts configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiNestedDonutsConfig : VueDataUiConfig;

/// <summary>Onion chart layer。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiOnionDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#percentage")]
    public double Percentage { get; init; }

    [Description("@#value")]
    public double? Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#prefix")]
    public string? Prefix { get; init; }

    [Description("@#suffix")]
    public string? Suffix { get; init; }
}

/// <summary>Onion chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiOnionConfig : VueDataUiConfig;

/// <summary>Parallel coordinate axis values。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiParallelCoordinatePlotDatasetSerieItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double?[] Values { get; init; } = [];
}

/// <summary>Parallel coordinate plot series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiParallelCoordinatePlotDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#shape")]
    public string? Shape { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#series")]
    public VueUiParallelCoordinatePlotDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Parallel coordinate plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiParallelCoordinatePlotConfig : VueDataUiConfig;

/// <summary>Quadrant point。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiQuadrantDatasetSerieItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#x")]
    public double X { get; init; }

    [Description("@#y")]
    public double Y { get; init; }
}

/// <summary>Quadrant dataset row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiQuadrantDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#shape")]
    public string? Shape { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#series")]
    public VueUiQuadrantDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Quadrant configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiQuadrantConfig : VueDataUiConfig;

/// <summary>Rating detailed scores keyed by label。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRatingDatasetDetailed : Vue.VueDictionary<double>;

/// <summary>Rating may be a scalar or a named score dictionary。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUiRatingValue(double, VueUiRatingDatasetDetailed)
{
    public double? AsNumber => Value is double value ? value : default(double?);

    public VueUiRatingDatasetDetailed? AsDetailed => Value as VueUiRatingDatasetDetailed;
}

/// <summary>Rating and Smiley dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRatingDataset : Vue.VueProps
{
    [Description("@#rating")]
    public VueUiRatingValue Rating { get; init; } = default!;
}

/// <summary>Rating configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRatingConfig : VueDataUiConfig;

/// <summary>Relation circle entity and its linked ids。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRelationCircleDatasetItem : VueDataUiDatasetItem
{
    [Description("@#id")]
    public Vue.VueStringNumberValue Id { get; init; } = default!;

    [Description("@#label")]
    public string Label { get; init; } = string.Empty;

    [Description("@#relations")]
    public Vue.VueStringNumberValue[] Relations { get; init; } = [];

    [Description("@#weights")]
    public double[]? Weights { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Relation circle configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRelationCircleConfig : VueDataUiConfig;

/// <summary>Ridgeline nested datapoint。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRidgelineDatapoint : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Ridgeline series group。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRidgelineDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#datapoints")]
    public VueUiRidgelineDatapoint[] Datapoints { get; init; } = [];
}

/// <summary>Ridgeline configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRidgelineConfig : VueDataUiConfig;

/// <summary>Rings chart series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRingsDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#values")]
    public double[] Values { get; init; } = [];
}

/// <summary>Rings chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRingsConfig : VueDataUiConfig;

/// <summary>Skeleton visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSkeletonConfig : VueDataUiConfig;

/// <summary>Smiley configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSmileyConfig : VueDataUiConfig;

/// <summary>Spark trend configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkTrendConfig : VueDataUiConfig;

/// <summary>Spark gauge dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkgaugeDataset : Vue.VueProps
{
    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#min")]
    public double Min { get; init; }

    [Description("@#max")]
    public double Max { get; init; }

    [Description("@#title")]
    public string? Title { get; init; }
}

/// <summary>Spark gauge configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkgaugeConfig : VueDataUiConfig;

/// <summary>Spark stackbar segment。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkStackbarDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#proportion")]
    public double? Proportion { get; init; }

    [Description("@#proportionLabel")]
    public string? ProportionLabel { get; init; }

    [Description("@#start")]
    public double? Start { get; init; }

    [Description("@#value")]
    public double? Value { get; init; }

    [Description("@#width")]
    public double? Width { get; init; }
}

/// <summary>Spark stackbar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkStackbarConfig : VueDataUiConfig;

/// <summary>Strip plot point。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiStripPlotDatasetItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }
}

/// <summary>Strip plot group。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiStripPlotDataset : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#plots")]
    public VueUiStripPlotDatasetItem[] Plots { get; init; } = [];
}

/// <summary>Strip plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiStripPlotConfig : VueDataUiConfig;

/// <summary>Thermometer gradient colors。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiThermometerColors : Vue.VueProps
{
    [Description("@#from")]
    public string? From { get; init; }

    [Description("@#to")]
    public string? To { get; init; }
}

/// <summary>Thermometer dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiThermometerDataset : Vue.VueProps
{
    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#from")]
    public double From { get; init; }

    [Description("@#to")]
    public double To { get; init; }

    [Description("@#steps")]
    public double? Steps { get; init; }

    [Description("@#colors")]
    public VueUiThermometerColors? Colors { get; init; }
}

/// <summary>Thermometer configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiThermometerConfig : VueDataUiConfig;

/// <summary>Timer configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTimerConfig : VueDataUiConfig;

/// <summary>Tiremarks dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTiremarksDataset : Vue.VueProps
{
    [Description("@#percentage")]
    public double Percentage { get; init; }
}

/// <summary>Tiremarks configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTiremarksConfig : VueDataUiConfig;

/// <summary>Wheel dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWheelDataset : Vue.VueProps
{
    [Description("@#percentage")]
    public double Percentage { get; init; }
}

/// <summary>Wheel configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWheelConfig : VueDataUiConfig;

/// <summary>World map country value。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWorldDatasetItem : Vue.VueProps
{
    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#category")]
    public string? Category { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>World map's ISO-keyed dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWorldDataset : Vue.VueDictionary<VueUiWorldDatasetItem>;

/// <summary>World map configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWorldConfig : VueDataUiConfig;

/// <summary>XY canvas series row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyCanvasDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#series")]
    public double?[] Series { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#type")]
    public VueUiXySeriesType? Type { get; init; }

    [Description("@#useArea")]
    public bool? UseArea { get; init; }

    [Description("@#dataLabels")]
    public bool? DataLabels { get; init; }

    [Description("@#scaleSteps")]
    public double? ScaleSteps { get; init; }

    [Description("@#prefix")]
    public string? Prefix { get; init; }

    [Description("@#suffix")]
    public string? Suffix { get; init; }

    [Description("@#rounding")]
    public double? Rounding { get; init; }

    [Description("@#autoScaling")]
    public bool? AutoScaling { get; init; }

    [Description("@#scaleMin")]
    public double? ScaleMin { get; init; }

    [Description("@#scaleMax")]
    public double? ScaleMax { get; init; }

    [Description("@#showYMarker")]
    public bool? ShowYMarker { get; init; }
}

/// <summary>XY canvas configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyCanvasConfig : VueDataUiConfig;

/// <summary>
/// vue-data-ui SVG pattern 的 closed literal domain。Description 保留 upstream 的 kebab-case
/// runtime token，避免 C# identifier 命名影响 emitted JavaScript。
/// </summary>
[String]
public enum VueUiPatternName
{
    [Description("@#bubbles")]
    Bubbles,

    [Description("@#flooring")]
    Flooring,

    [Description("@#grid")]
    Grid,

    [Description("@#hexagon-diamond")]
    HexagonDiamond,

    [Description("@#hexagon-flooring")]
    HexagonFlooring,

    [Description("@#hexagon-grid")]
    HexagonGrid,

    [Description("@#maze")]
    Maze,

    [Description("@#redrum")]
    Redrum,

    [Description("@#scales")]
    Scales,

    [Description("@#squares")]
    Squares,

    [Description("@#wave")]
    Wave,

    [Description("@#zig-zag")]
    ZigZag
}

/// <summary>
/// vue-data-ui 内置 icon 的 closed literal domain。成员与 3.23.4 declaration 对齐；运行时
/// token 由 Description 固定，新增上游 icon 时应在 catalog parity 测试更新时同步补齐。
/// </summary>
[String]
public enum VueUiIconName
{
    [Description("@#accessibility")]
    Accessibility,

    [Description("@#addColumn")]
    AddColumn,

    [Description("@#addRow")]
    AddRow,

    [Description("@#aToZ")]
    AToZ,

    [Description("@#accordion")]
    Accordion,

    [Description("@#annotation")]
    Annotation,

    [Description("@#annotator")]
    Annotator,

    [Description("@#annotatorDisabled")]
    AnnotatorDisabled,

    [Description("@#apiStream")]
    ApiStream,

    [Description("@#arrowBottom")]
    ArrowBottom,

    [Description("@#arrowLeft")]
    ArrowLeft,

    [Description("@#arrowRight")]
    ArrowRight,

    [Description("@#arrowTop")]
    ArrowTop,

    [Description("@#battery")]
    Battery,

    [Description("@#bell")]
    Bell,

    [Description("@#bellOff")]
    BellOff,

    [Description("@#bellRing")]
    BellRing,

    [Description("@#binary")]
    Binary,

    [Description("@#blur")]
    Blur,

    [Description("@#boxes")]
    Boxes,

    [Description("@#branches")]
    Branches,

    [Description("@#bringToBack")]
    BringToBack,

    [Description("@#bringToFront")]
    BringToFront,

    [Description("@#bucket")]
    Bucket,

    [Description("@#bucketEmpty")]
    BucketEmpty,

    [Description("@#bucketFill")]
    BucketFill,

    [Description("@#bucketRecycle")]
    BucketRecycle,

    [Description("@#bug")]
    Bug,

    [Description("@#building")]
    Building,

    [Description("@#calendar")]
    Calendar,

    [Description("@#carouselTable")]
    CarouselTable,

    [Description("@#chart3dBar")]
    Chart3dBar,

    [Description("@#chartAgePyramid")]
    ChartAgePyramid,

    [Description("@#chartBar")]
    ChartBar,

    [Description("@#chartBullet")]
    ChartBullet,

    [Description("@#chartBump")]
    ChartBump,

    [Description("@#chartCandlestick")]
    ChartCandlestick,

    [Description("@#chartChestnut")]
    ChartChestnut,

    [Description("@#chartChord")]
    ChartChord,

    [Description("@#chartCirclePack")]
    ChartCirclePack,

    [Description("@#chartCluster")]
    ChartCluster,

    [Description("@#chartDag")]
    ChartDag,

    [Description("@#chartDonut")]
    ChartDonut,

    [Description("@#chartDonutEvolution")]
    ChartDonutEvolution,

    [Description("@#chartDumbbell")]
    ChartDumbbell,

    [Description("@#chartFlow")]
    ChartFlow,

    [Description("@#chartFunnel")]
    ChartFunnel,

    [Description("@#chartGalaxy")]
    ChartGalaxy,

    [Description("@#chartGauge")]
    ChartGauge,

    [Description("@#chartHeatmap")]
    ChartHeatmap,

    [Description("@#chartHill")]
    ChartHill,

    [Description("@#chartHistoryPlot")]
    ChartHistoryPlot,

    [Description("@#chartLine")]
    ChartLine,

    [Description("@#chartMoodRadar")]
    ChartMoodRadar,

    [Description("@#chartNestedDonuts")]
    ChartNestedDonuts,

    [Description("@#chartOnion")]
    ChartOnion,

    [Description("@#chartParallelCoordinatePlot")]
    ChartParallelCoordinatePlot,

    [Description("@#chartQuadrant")]
    ChartQuadrant,

    [Description("@#chartRadar")]
    ChartRadar,

    [Description("@#chartRelationCircle")]
    ChartRelationCircle,

    [Description("@#chartRidgeline")]
    ChartRidgeline,

    [Description("@#chartRings")]
    ChartRings,

    [Description("@#chartScatter")]
    ChartScatter,

    [Description("@#chartSparkHistogram")]
    ChartSparkHistogram,

    [Description("@#chartSparkStackbar")]
    ChartSparkStackbar,

    [Description("@#chartSparkbar")]
    ChartSparkbar,

    [Description("@#chartSparkline")]
    ChartSparkline,

    [Description("@#chartStackbar")]
    ChartStackbar,

    [Description("@#chartStackline")]
    ChartStackline,

    [Description("@#chartStripPlot")]
    ChartStripPlot,

    [Description("@#chartTable")]
    ChartTable,

    [Description("@#chartTableSparkline")]
    ChartTableSparkline,

    [Description("@#chartThermometer")]
    ChartThermometer,

    [Description("@#chartTiremarks")]
    ChartTiremarks,

    [Description("@#chartVerticalBar")]
    ChartVerticalBar,

    [Description("@#chartWaffle")]
    ChartWaffle,

    [Description("@#chartWheel")]
    ChartWheel,

    [Description("@#chartWordCloud")]
    ChartWordCloud,

    [Description("@#chartWordCloudZh")]
    ChartWordCloudZh,

    [Description("@#check")]
    Check,

    [Description("@#checkList")]
    CheckList,

    [Description("@#chip")]
    Chip,

    [Description("@#chipAi")]
    ChipAi,

    [Description("@#chipBinary")]
    ChipBinary,

    [Description("@#circle")]
    Circle,

    [Description("@#circleCancel")]
    CircleCancel,

    [Description("@#circleCheck")]
    CircleCheck,

    [Description("@#circleExclamation")]
    CircleExclamation,

    [Description("@#circleFill")]
    CircleFill,

    [Description("@#circleQuestion")]
    CircleQuestion,

    [Description("@#clankerCrazy")]
    ClankerCrazy,

    [Description("@#clankerNasty")]
    ClankerNasty,

    [Description("@#clip")]
    Clip,

    [Description("@#clipBoard")]
    ClipBoard,

    [Description("@#clipboardBar")]
    ClipboardBar,

    [Description("@#clipboardDonut")]
    ClipboardDonut,

    [Description("@#clipboardLine")]
    ClipboardLine,

    [Description("@#clipboardVariable")]
    ClipboardVariable,

    [Description("@#close")]
    Close,

    [Description("@#cloud")]
    Cloud,

    [Description("@#cloudRain")]
    CloudRain,

    [Description("@#colorPicker")]
    ColorPicker,

    [Description("@#computer")]
    Computer,

    [Description("@#copy")]
    Copy,

    [Description("@#copyLeft")]
    CopyLeft,

    [Description("@#croissant")]
    Croissant,

    [Description("@#csv")]
    Csv,

    [Description("@#curlyBrackets")]
    CurlyBrackets,

    [Description("@#curlySpread")]
    CurlySpread,

    [Description("@#cursor")]
    Cursor,

    [Description("@#dashboard")]
    Dashboard,

    [Description("@#database")]
    Database,

    [Description("@#diamond")]
    Diamond,

    [Description("@#diamondFill")]
    DiamondFill,

    [Description("@#digit0")]
    Digit0,

    [Description("@#digit1")]
    Digit1,

    [Description("@#digit2")]
    Digit2,

    [Description("@#digit3")]
    Digit3,

    [Description("@#digit4")]
    Digit4,

    [Description("@#digit5")]
    Digit5,

    [Description("@#digit6")]
    Digit6,

    [Description("@#digit7")]
    Digit7,

    [Description("@#digit8")]
    Digit8,

    [Description("@#digit9")]
    Digit9,

    [Description("@#direction")]
    Direction,

    [Description("@#document")]
    Document,

    [Description("@#doubleCheck")]
    DoubleCheck,

    [Description("@#doubleSpark")]
    DoubleSpark,

    [Description("@#download")]
    Download,

    [Description("@#envelope")]
    Envelope,

    [Description("@#excel")]
    Excel,

    [Description("@#exitFullscreen")]
    ExitFullscreen,

    [Description("@#export")]
    Export,

    [Description("@#externalLink")]
    ExternalLink,

    [Description("@#eye")]
    Eye,

    [Description("@#file")]
    File,

    [Description("@#fileCsv")]
    FileCsv,

    [Description("@#filePdf")]
    FilePdf,

    [Description("@#filePlus")]
    FilePlus,

    [Description("@#filePng")]
    FilePng,

    [Description("@#fileSvg")]
    FileSvg,

    [Description("@#fileSearch")]
    FileSearch,

    [Description("@#focus")]
    Focus,

    [Description("@#folder")]
    Folder,

    [Description("@#folderFill")]
    FolderFill,

    [Description("@#folderOpen")]
    FolderOpen,

    [Description("@#folderOpenFill")]
    FolderOpenFill,

    [Description("@#fork")]
    Fork,

    [Description("@#frameLine")]
    FrameLine,

    [Description("@#fullscreen")]
    Fullscreen,

    [Description("@#func")]
    Func,

    [Description("@#gisLayerQuery")]
    GisLayerQuery,

    [Description("@#gisLayerSearch")]
    GisLayerSearch,

    [Description("@#hexagon")]
    Hexagon,

    [Description("@#hexagonFill")]
    HexagonFill,

    [Description("@#hierarchy")]
    Hierarchy,

    [Description("@#histogram")]
    Histogram,

    [Description("@#histogramDown")]
    HistogramDown,

    [Description("@#histogramUp")]
    HistogramUp,

    [Description("@#home")]
    Home,

    [Description("@#homeFilled")]
    HomeFilled,

    [Description("@#hourglass")]
    Hourglass,

    [Description("@#htmlTag")]
    HtmlTag,

    [Description("@#icons")]
    Icons,

    [Description("@#image")]
    Image,

    [Description("@#key")]
    Key,

    [Description("@#knobs")]
    Knobs,

    [Description("@#kpi")]
    Kpi,

    [Description("@#kpiBox")]
    KpiBox,

    [Description("@#labelClose")]
    LabelClose,

    [Description("@#labelOpen")]
    LabelOpen,

    [Description("@#lambda")]
    Lambda,

    [Description("@#lap")]
    Lap,

    [Description("@#laptop")]
    Laptop,

    [Description("@#legend")]
    Legend,

    [Description("@#lightBulbOff")]
    LightBulbOff,

    [Description("@#lightBulbOn")]
    LightBulbOn,

    [Description("@#lineUp")]
    LineUp,

    [Description("@#link")]
    Link,

    [Description("@#listType")]
    ListType,

    [Description("@#lock")]
    Lock,

    [Description("@#magnify")]
    Magnify,

    [Description("@#menu")]
    Menu,

    [Description("@#microscope")]
    Microscope,

    [Description("@#minimap")]
    Minimap,

    [Description("@#minus")]
    Minus,

    [Description("@#monitor")]
    Monitor,

    [Description("@#moodEmbarrassed")]
    MoodEmbarrassed,

    [Description("@#moodFlat")]
    MoodFlat,

    [Description("@#moodHappy")]
    MoodHappy,

    [Description("@#moodLaughing")]
    MoodLaughing,

    [Description("@#moodNeutral")]
    MoodNeutral,

    [Description("@#moodSad")]
    MoodSad,

    [Description("@#moodSurprised")]
    MoodSurprised,

    [Description("@#moodWink")]
    MoodWink,

    [Description("@#move")]
    Move,

    [Description("@#mu")]
    Mu,

    [Description("@#network")]
    Network,

    [Description("@#nineToZero")]
    NineToZero,

    [Description("@#npmx")]
    Npmx,

    [Description("@#numbers")]
    Numbers,

    [Description("@#palette")]
    Palette,

    [Description("@#pause")]
    Pause,

    [Description("@#pdf")]
    Pdf,

    [Description("@#pentagon")]
    Pentagon,

    [Description("@#pentagonFill")]
    PentagonFill,

    [Description("@#people")]
    People,

    [Description("@#percentage")]
    Percentage,

    [Description("@#percentageDown")]
    PercentageDown,

    [Description("@#percentageUp")]
    PercentageUp,

    [Description("@#person")]
    Person,

    [Description("@#pi")]
    Pi,

    [Description("@#pie")]
    Pie,

    [Description("@#play")]
    Play,

    [Description("@#plotArrow")]
    PlotArrow,

    [Description("@#plotLine")]
    PlotLine,

    [Description("@#plug")]
    Plug,

    [Description("@#plus")]
    Plus,

    [Description("@#pointer")]
    Pointer,

    [Description("@#printer")]
    Printer,

    [Description("@#puzzle")]
    Puzzle,

    [Description("@#puzzleFill")]
    PuzzleFill,

    [Description("@#ratio")]
    Ratio,

    [Description("@#recycle")]
    Recycle,

    [Description("@#refresh")]
    Refresh,

    [Description("@#resize")]
    Resize,

    [Description("@#resizeTLBR")]
    ResizeTLBR,

    [Description("@#resizeTRBL")]
    ResizeTRBL,

    [Description("@#resizeX")]
    ResizeX,

    [Description("@#resizeY")]
    ResizeY,

    [Description("@#restart")]
    Restart,

    [Description("@#revert")]
    Revert,

    [Description("@#robot")]
    Robot,

    [Description("@#save")]
    Save,

    [Description("@#scada")]
    Scada,

    [Description("@#screenshot")]
    Screenshot,

    [Description("@#selectAndGroup")]
    SelectAndGroup,

    [Description("@#settings")]
    Settings,

    [Description("@#shield")]
    Shield,

    [Description("@#shieldExclam")]
    ShieldExclam,

    [Description("@#sigma")]
    Sigma,

    [Description("@#skeleton")]
    Skeleton,

    [Description("@#sliders")]
    Sliders,

    [Description("@#smiley")]
    Smiley,

    [Description("@#sort")]
    Sort,

    [Description("@#spark")]
    Spark,

    [Description("@#spin")]
    Spin,

    [Description("@#spinner1")]
    Spinner1,

    [Description("@#spinner2")]
    Spinner2,

    [Description("@#spinner3")]
    Spinner3,

    [Description("@#spinner4")]
    Spinner4,

    [Description("@#sql")]
    Sql,

    [Description("@#sqlQuery")]
    SqlQuery,

    [Description("@#sqlSearch")]
    SqlSearch,

    [Description("@#square")]
    Square,

    [Description("@#squareFill")]
    SquareFill,

    [Description("@#stack")]
    Stack,

    [Description("@#star")]
    Star,

    [Description("@#starFace")]
    StarFace,

    [Description("@#starFill")]
    StarFill,

    [Description("@#stop")]
    Stop,

    [Description("@#sun")]
    Sun,

    [Description("@#svg")]
    Svg,

    [Description("@#tableClose")]
    TableClose,

    [Description("@#tableDialogClose")]
    TableDialogClose,

    [Description("@#tableDialogOpen")]
    TableDialogOpen,

    [Description("@#tableOpen")]
    TableOpen,

    [Description("@#tag")]
    Tag,

    [Description("@#target")]
    Target,

    [Description("@#test")]
    Test,

    [Description("@#text")]
    Text,

    [Description("@#tooltip")]
    Tooltip,

    [Description("@#tooltipDisabled")]
    TooltipDisabled,

    [Description("@#trash")]
    Trash,

    [Description("@#trend")]
    Trend,

    [Description("@#trendDown")]
    TrendDown,

    [Description("@#trendUp")]
    TrendUp,

    [Description("@#triangle")]
    Triangle,

    [Description("@#triangleExclamation")]
    TriangleExclamation,

    [Description("@#triangleFill")]
    TriangleFill,

    [Description("@#triangleInformation")]
    TriangleInformation,

    [Description("@#twig")]
    Twig,

    [Description("@#unlock")]
    Unlock,

    [Description("@#unplug")]
    Unplug,

    [Description("@#unstack")]
    Unstack,

    [Description("@#upload")]
    Upload,

    [Description("@#vueDataUi")]
    VueDataUi,

    [Description("@#wifi")]
    Wifi,

    [Description("@#world")]
    World,

    [Description("@#wrench")]
    Wrench,

    [Description("@#zToA")]
    ZToA,

    [Description("@#zeroToNine")]
    ZeroToNine,

    [Description("@#zoomLock")]
    ZoomLock,

    [Description("@#zoomMinus")]
    ZoomMinus,

    [Description("@#zoomPlus")]
    ZoomPlus,

    [Description("@#zoomUnlock")]
    ZoomUnlock
}
