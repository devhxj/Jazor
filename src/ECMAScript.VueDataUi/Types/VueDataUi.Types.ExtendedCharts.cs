namespace ECMAScript.VueDataUi;

// 3.23.4 complete catalog: these records intentionally model each component's stable input shape.
// Deep style/options remain extensible through VdConfig/VdDatasetItem, never object.

/// <summary>Vd3dBar 的 breakdown 条目。</summary>
[ECMAScript]
[Description("@#")]
public record Vd3dBarDatasetBreakdown : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }
}

/// <summary>Vd3dBar 的单个 series。</summary>
[ECMAScript]
[Description("@#")]
public record Vd3dBarDatasetSeriesItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 当前项数值的细分数据，用于绘制构成部分。
    /// </summary>
    [Description("@#breakdown")]
    public Vd3dBarDatasetBreakdown[]? Breakdown { get; init; }
}

/// <summary>Vd3dBar dataset 根对象。</summary>
[ECMAScript]
[Description("@#")]
public record Vd3dBarDataset : Vue.VueProps
{
    /// <summary>
    /// 用于绘制百分比进度的数值，通常以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public double? Percentage { get; init; }

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public Vd3dBarDatasetSeriesItem[]? Series { get; init; }
}

/// <summary>3D bar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record Vd3dBarConfig : VdConfig;

/// <summary>Accordion configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdAccordionConfig : VdConfig;

/// <summary>Age pyramid configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdAgePyramidConfig : VdConfig;

/// <summary>
/// Age pyramid positional row helper。C# tuple 会 lower 为 object，所以此 helper owns the array shape
/// required by upstream: <c>[year, rank, left, right]</c>。
/// </summary>
public static class VdAgePyramidData
{
    /// <summary>
    /// 按 [year, rank, left, right] 顺序构造人口金字塔数据行；left/right 为两侧分组数值。
    /// </summary>
    [ECMAScriptInline("[__arg1, __arg2, __arg3, __arg4]")]
    public extern static VdCellValue[] Row(string year, double rank, double? left, double? right);
}

/// <summary>Annotator 可选 dataset 的结构化 object。</summary>
[ECMAScript]
[Description("@#")]
public record VdAnnotatorDataset : VdDatasetItem;

/// <summary>Annotator configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdAnnotatorConfig : VdConfig;

/// <summary>Bump chart 的 series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdBumpDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Bump chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdBumpConfig : VdConfig;

/// <summary>Carousel table dataset。每行 cell 保持 string/number closed domain。</summary>
[ECMAScript]
[Description("@#")]
public record VdCarouselTableDataset : Vue.VueProps
{
    /// <summary>
    /// 按列顺序排列的表头文本。
    /// </summary>
    [Description("@#head")]
    public string[] Head { get; init; } = [];

    /// <summary>
    /// 表格正文的行数据，每行单元格顺序应与表头一致。
    /// </summary>
    [Description("@#body")]
    public VdCellValue[][] Body { get; init; } = [];
}

/// <summary>Carousel table configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdCarouselTableConfig : VdConfig;

/// <summary>Chestnut chart breakdown leaf。</summary>
[ECMAScript]
[Description("@#")]
public record VdChestnutDatasetBranchBreakdown : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Chestnut chart branch。</summary>
[ECMAScript]
[Description("@#")]
public record VdChestnutDatasetBranch : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 当前项数值的细分数据，用于绘制构成部分。
    /// </summary>
    [Description("@#breakdown")]
    public VdChestnutDatasetBranchBreakdown[]? Breakdown { get; init; }
}

/// <summary>Chestnut chart root node。</summary>
[ECMAScript]
[Description("@#")]
public record VdChestnutDatasetRoot : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 根节点包含的分支数据。
    /// </summary>
    [Description("@#branches")]
    public VdChestnutDatasetBranch[] Branches { get; init; } = [];
}

/// <summary>Chestnut chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdChestnutConfig : VdConfig;

/// <summary>Chord matrix dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VdChordDataset : Vue.VueProps
{
    /// <summary>
    /// 弦图关系矩阵；行列索引对应 labels 中的节点顺序。
    /// </summary>
    [Description("@#matrix")]
    public double?[][] Matrix { get; init; } = [];

    /// <summary>
    /// 按矩阵行列顺序排列的节点名称。
    /// </summary>
    [Description("@#labels")]
    public string[]? Labels { get; init; }

    /// <summary>
    /// 按节点顺序指定的 CSS 颜色数组。
    /// </summary>
    [Description("@#colors")]
    public string[]? Colors { get; init; }
}

/// <summary>Chord chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdChordConfig : VdConfig;

/// <summary>Circle pack hierarchy node。</summary>
[ECMAScript]
[Description("@#")]
public record VdCirclePackDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 当前数据项的子节点，用于形成层级或分类细分。
    /// </summary>
    [Description("@#children")]
    public VdCirclePackDatasetItem[]? Children { get; init; }
}

/// <summary>Circle pack configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdCirclePackConfig : VdConfig;

/// <summary>Cursor visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdCursorConfig : VdConfig;

/// <summary>DAG node authored by callers。额外 metadata 可通过继承的 dictionary 传递。</summary>
[ECMAScript]
[Description("@#")]
public record VdDagNode : VdDatasetItem
{
    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 此项显示的文本标签。
    /// </summary>
    [Description("@#label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// 背景的 CSS 颜色值。
    /// </summary>
    [Description("@#backgroundColor")]
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>DAG edge authored by callers。</summary>
[ECMAScript]
[Description("@#")]
public record VdDagEdge : Vue.VueProps
{
    /// <summary>
    /// 有向边的起始节点 id。
    /// </summary>
    [Description("@#from")]
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// 有向边的目标节点 id。
    /// </summary>
    [Description("@#to")]
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 是否为此有向边启用流动动画。
    /// </summary>
    [Description("@#animated")]
    public bool? Animated { get; init; }

    /// <summary>
    /// SVG stroke-dasharray，控制边线的虚线段和间隔。
    /// </summary>
    [Description("@#dasharray")]
    public string? Dasharray { get; init; }

    /// <summary>
    /// 边线动画的持续时间，单位为毫秒。
    /// </summary>
    [Description("@#animationDurationMs")]
    public double? AnimationDurationMs { get; init; }

    /// <summary>
    /// 边线动画的方向数值；按上游组件对方向的约定传入。
    /// </summary>
    [Description("@#animationDirection")]
    public double? AnimationDirection { get; init; }
}

/// <summary>DAG dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdDagDataset : Vue.VueProps
{
    /// <summary>
    /// 图中的节点；每个节点的 id 用于边的 from/to 引用。
    /// </summary>
    [Description("@#nodes")]
    public VdDagNode[] Nodes { get; init; } = [];

    /// <summary>
    /// 连接 nodes 中节点的有向边；from/to 应引用节点的 id。
    /// </summary>
    [Description("@#edges")]
    public VdDagEdge[] Edges { get; init; } = [];
}

/// <summary>DAG chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdDagConfig : VdConfig;

/// <summary>
/// Dashboard item props base。具体 chart 可使用 <see cref="VdDashboardElementProps{TDataset,TConfig}"/>
/// 保持内部 dataset/config 的 exact C# type。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardElementProps : Vue.VueProps;

/// <summary>Dashboard 中一个具体图表的 typed props。</summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardElementProps<TDataset, TConfig> : VdDashboardElementProps
{
    /// <summary>
    /// 传入组件的数据；数据形状由当前图表的 Dataset 类型决定。
    /// </summary>
    [Description("@#dataset")]
    public TDataset Dataset { get; init; } = default!;

    /// <summary>
    /// 组件的显示和交互配置；具体选项由当前图表的强类型配置定义。
    /// </summary>
    [Description("@#config")]
    public TConfig? Config { get; init; }
}

/// <summary>Dashboard grid 中的一个 component placement。</summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardElement : VdDatasetItem
{
    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public Vue.VueStringNumberValue Id { get; init; } = default!;

    /// <summary>
    /// 显示宽度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#width")]
    public double Width { get; init; }

    /// <summary>
    /// 显示高度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#height")]
    public double Height { get; init; }

    /// <summary>
    /// 仪表板元素相对于布局容器的横向位置。
    /// </summary>
    [Description("@#left")]
    public double Left { get; init; }

    /// <summary>
    /// 仪表板元素相对于布局容器的纵向位置。
    /// </summary>
    [Description("@#top")]
    public double Top { get; init; }

    /// <summary>
    /// 内嵌组件的名称；用于选择仪表板元素对应的图表渲染器。
    /// </summary>
    [Description("@#component")]
    public string Component { get; init; } = string.Empty;

    /// <summary>
    /// 传给仪表板内嵌图表的参数，包含 dataset 和 config。
    /// </summary>
    [Description("@#props")]
    public VdDashboardElementProps? Props { get; init; }
}

/// <summary>Dashboard <c>change</c> event 的 placement shape。</summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardPlacedElement : Vue.VueProps
{
    /// <summary>
    /// 内嵌组件的名称；用于选择仪表板元素对应的图表渲染器。
    /// </summary>
    [Description("@#component")]
    public string Component { get; init; } = string.Empty;

    /// <summary>
    /// 显示高度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#height")]
    public double Height { get; init; }

    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 当前组件在仪表板布局集合中的索引。
    /// </summary>
    [Description("@#index")]
    public double Index { get; init; }

    /// <summary>
    /// 仪表板元素相对于布局容器的横向位置。
    /// </summary>
    [Description("@#left")]
    public double Left { get; init; }

    /// <summary>
    /// 仪表板元素相对于布局容器的纵向位置。
    /// </summary>
    [Description("@#top")]
    public double Top { get; init; }

    /// <summary>
    /// 显示宽度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#width")]
    public double Width { get; init; }
}

/// <summary>Dashboard <c>copyAlt</c> event payload。</summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardCopyAlt : Vue.VueProps
{
    /// <summary>
    /// 组件的显示和交互配置；具体选项由当前图表的强类型配置定义。
    /// </summary>
    [Description("@#config")]
    public VdDashboardConfig Config { get; init; } = default!;

    /// <summary>
    /// 传入组件的数据；数据形状由当前图表的 Dataset 类型决定。
    /// </summary>
    [Description("@#dataset")]
    public VdDashboardPlacedElement[] Dataset { get; init; } = [];
}

/// <summary>Dashboard configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdDashboardConfig : VdConfig;

/// <summary>Digits visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdDigitsConfig : VdConfig
{
    /// <summary>
    /// 背景的 CSS 颜色值。
    /// </summary>
    [Description("@#backgroundColor")]
    public string? BackgroundColor { get; init; }

    /// <summary>
    /// 显示高度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#height")]
    public string? Height { get; init; }

    /// <summary>
    /// 显示宽度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#width")]
    public string? Width { get; init; }
}

/// <summary>Donut evolution 的 series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdDonutEvolutionDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Donut evolution configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdDonutEvolutionConfig : VdConfig;

/// <summary>
/// Flow link array helper。The upstream tuple must remain a JavaScript array rather than a C# tuple object.
/// </summary>
public static class VdFlowData
{
    /// <summary>
    /// 按 [from, to, value] 顺序构造流向图连线；from/to 为节点名称，value 为流量。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <param name="from">连接起点的节点标识，需与 Nodes 中的节点名称对应。</param>
    /// <param name="to">连接终点的节点标识，需与 Nodes 中的节点名称对应。</param>
    [ECMAScriptInline("[__arg1, __arg2, __arg3]")]
    public extern static VdCellValue[] Link(string from, string to, double? value);
}

/// <summary>Flow chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdFlowConfig : VdConfig;

/// <summary>Galaxy configuration。Dataset reuses <see cref="VdDonutDatasetItem"/> rows.</summary>
[ECMAScript]
[Description("@#")]
public record VdGalaxyConfig : VdConfig;

/// <summary>Geo map point。Coordinates are emitted as a two-item JavaScript array.</summary>
[ECMAScript]
[Description("@#")]
public record VdGeoDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 地理坐标数组；按 Geo 组件要求提供经度、纬度。
    /// </summary>
    [Description("@#coordinates")]
    public double[] Coordinates { get; init; } = [];

    /// <summary>
    /// 地理数据点的补充说明文字。
    /// </summary>
    [Description("@#description")]
    public string? Description { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 地理数据点的显示半径。
    /// </summary>
    [Description("@#radius")]
    public double? Radius { get; init; }
}

/// <summary>Geo map configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdGeoConfig : VdConfig;

/// <summary>Gizmo configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdGizmoConfig : VdConfig;

/// <summary>Hill chart item。The chart deliberately accepts arbitrary metadata alongside these stable fields.</summary>
[ECMAScript]
[Description("@#")]
public record VdHillDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 此项显示的文本标签。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 以弱化的视觉样式显示此数据项。
    /// </summary>
    [Description("@#muted")]
    public bool? Muted { get; init; }

    /// <summary>
    /// 禁用此数据项的交互。
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>Hill chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdHillConfig : VdConfig;

/// <summary>History plot coordinate。</summary>
[ECMAScript]
[Description("@#")]
public record VdHistoryPlotValue : Vue.VueProps
{
    /// <summary>
    /// 数据点在 X 轴上的数值坐标。
    /// </summary>
    [Description("@#x")]
    public double X { get; init; }

    /// <summary>
    /// 数据点在 Y 轴上的数值坐标。
    /// </summary>
    [Description("@#y")]
    public double Y { get; init; }

    /// <summary>
    /// 此项显示的文本标签。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }
}

/// <summary>History plot series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdHistoryPlotDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public VdHistoryPlotValue[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 使用平滑曲线连接序列中的相邻数据点。
    /// </summary>
    [Description("@#smooth")]
    public bool? Smooth { get; init; }

    /// <summary>
    /// 根据数据变化绘制温度色带时使用的颜色序列。
    /// </summary>
    [Description("@#temperatureColors")]
    public string[]? TemperatureColors { get; init; }

    /// <summary>
    /// 温度颜色渐变的方向角度。
    /// </summary>
    [Description("@#temperatureAngle")]
    public double? TemperatureAngle { get; init; }

    /// <summary>
    /// 让数据点使用温度配色。
    /// </summary>
    [Description("@#usePlotTemperatureColors")]
    public bool? UsePlotTemperatureColors { get; init; }

    /// <summary>
    /// 单独计算此序列的温度配色范围，而不共享其他序列的范围。
    /// </summary>
    [Description("@#temperatureIndependant")]
    public bool? TemperatureIndependant { get; init; }
}

/// <summary>History plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdHistoryPlotConfig : VdConfig;

/// <summary>Mini loader configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdMiniLoaderConfig : VdConfig;

/// <summary>Molecule graph node。</summary>
[ECMAScript]
[Description("@#")]
public record VdMoleculeDatasetNode : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 节点的补充详细说明。
    /// </summary>
    [Description("@#details")]
    public string? Details { get; init; }

    /// <summary>
    /// 与当前分子节点连接的下一层节点。
    /// </summary>
    [Description("@#nodes")]
    public VdMoleculeDatasetNode[]? Nodes { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Molecule graph configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdMoleculeConfig : VdConfig;

/// <summary>Mood radar's fixed five score buckets。</summary>
[ECMAScript]
[Description("@#")]
public record VdMoodRadarDataset : Vue.VueProps
{
    /// <summary>
    /// 评分为 1 的数量，用于心情雷达图对应维度。
    /// </summary>
    [Description("@#1")]
    public double One { get; init; }

    /// <summary>
    /// 评分为 2 的数量，用于心情雷达图对应维度。
    /// </summary>
    [Description("@#2")]
    public double Two { get; init; }

    /// <summary>
    /// 评分为 3 的数量，用于心情雷达图对应维度。
    /// </summary>
    [Description("@#3")]
    public double Three { get; init; }

    /// <summary>
    /// 评分为 4 的数量，用于心情雷达图对应维度。
    /// </summary>
    [Description("@#4")]
    public double Four { get; init; }

    /// <summary>
    /// 评分为 5 的数量，用于心情雷达图对应维度。
    /// </summary>
    [Description("@#5")]
    public double Five { get; init; }
}

/// <summary>Mood radar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdMoodRadarConfig : VdConfig;

/// <summary>Nested donut ring and its inner donut series。</summary>
[ECMAScript]
[Description("@#")]
public record VdNestedDonutsDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VdDonutDatasetItem[] Series { get; init; } = [];
}

/// <summary>Nested donuts configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdNestedDonutsConfig : VdConfig;

/// <summary>Onion chart layer。</summary>
[ECMAScript]
[Description("@#")]
public record VdOnionDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 用于绘制百分比进度的数值，通常以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public double Percentage { get; init; }

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double? Value { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 显示数值时添加的文本前缀，不改变原始数据值。
    /// </summary>
    [Description("@#prefix")]
    public string? Prefix { get; init; }

    /// <summary>
    /// 显示数值时添加的文本后缀，不改变原始数据值。
    /// </summary>
    [Description("@#suffix")]
    public string? Suffix { get; init; }
}

/// <summary>Onion chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdOnionConfig : VdConfig;

/// <summary>Parallel coordinate axis values。</summary>
[ECMAScript]
[Description("@#")]
public record VdParallelCoordinatePlotDatasetSerieItem : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double?[] Values { get; init; } = [];
}

/// <summary>Parallel coordinate plot series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdParallelCoordinatePlotDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 数据点或热力单元格的形状；使用当前组件支持的形状值。
    /// </summary>
    [Description("@#shape")]
    public string? Shape { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VdParallelCoordinatePlotDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Parallel coordinate plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdParallelCoordinatePlotConfig : VdConfig;

/// <summary>Quadrant point。</summary>
[ECMAScript]
[Description("@#")]
public record VdQuadrantDatasetSerieItem : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 数据点在 X 轴上的数值坐标。
    /// </summary>
    [Description("@#x")]
    public double X { get; init; }

    /// <summary>
    /// 数据点在 Y 轴上的数值坐标。
    /// </summary>
    [Description("@#y")]
    public double Y { get; init; }
}

/// <summary>Quadrant dataset row。</summary>
[ECMAScript]
[Description("@#")]
public record VdQuadrantDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 数据点或热力单元格的形状；使用当前组件支持的形状值。
    /// </summary>
    [Description("@#shape")]
    public string? Shape { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VdQuadrantDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Quadrant configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdQuadrantConfig : VdConfig;

/// <summary>Rating detailed scores keyed by label。</summary>
[ECMAScript]
[Description("@#")]
public record VdRatingDatasetDetailed : Vue.VueDictionary<double>;

/// <summary>Rating may be a scalar or a named score dictionary。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VdRatingValue(double, VdRatingDatasetDetailed)
{
    /// <summary>
    /// 读取当前值的 double 分支；不属于该分支时返回 null。
    /// </summary>
    public double? AsNumber => Value is double value ? value : default(double?);

    /// <summary>
    /// 读取当前值的 VdRatingDatasetDetailed 分支；不属于该分支时返回 null。
    /// </summary>
    public VdRatingDatasetDetailed? AsDetailed => Value as VdRatingDatasetDetailed;
}

/// <summary>Rating and Smiley dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdRatingDataset : Vue.VueProps
{
    /// <summary>
    /// 当前评分值或按等级细分的评分数据。
    /// </summary>
    [Description("@#rating")]
    public VdRatingValue Rating { get; init; } = default!;
}

/// <summary>Rating configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdRatingConfig : VdConfig;

/// <summary>Relation circle entity and its linked ids。</summary>
[ECMAScript]
[Description("@#")]
public record VdRelationCircleDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public Vue.VueStringNumberValue Id { get; init; } = default!;

    /// <summary>
    /// 此项显示的文本标签。
    /// </summary>
    [Description("@#label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// 与当前节点连接的其他节点 id。
    /// </summary>
    [Description("@#relations")]
    public Vue.VueStringNumberValue[] Relations { get; init; } = [];

    /// <summary>
    /// 各条关系的权重；顺序与 relations 一致。
    /// </summary>
    [Description("@#weights")]
    public double[]? Weights { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Relation circle configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdRelationCircleConfig : VdConfig;

/// <summary>Ridgeline nested datapoint。</summary>
[ECMAScript]
[Description("@#")]
public record VdRidgelineDatapoint : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Ridgeline series group。</summary>
[ECMAScript]
[Description("@#")]
public record VdRidgelineDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 组成此山脊分布的数据点集合。
    /// </summary>
    [Description("@#datapoints")]
    public VdRidgelineDatapoint[] Datapoints { get; init; } = [];
}

/// <summary>Ridgeline configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdRidgelineConfig : VdConfig;

/// <summary>Rings chart series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdRingsDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double[] Values { get; init; } = [];
}

/// <summary>Rings chart configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdRingsConfig : VdConfig;

/// <summary>Skeleton visual configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdSkeletonConfig : VdConfig;

/// <summary>Smiley configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdSmileyConfig : VdConfig;

/// <summary>Spark trend configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdSparkTrendConfig : VdConfig;

/// <summary>Spark gauge dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdSparkgaugeDataset : Vue.VueProps
{
    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 仪表盘刻度的最小值。
    /// </summary>
    [Description("@#min")]
    public double Min { get; init; }

    /// <summary>
    /// 仪表盘刻度的最大值。
    /// </summary>
    [Description("@#max")]
    public double Max { get; init; }

    /// <summary>
    /// 图表或指标的标题文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }
}

/// <summary>Spark gauge configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdSparkgaugeConfig : VdConfig;

/// <summary>Spark stackbar segment。</summary>
[ECMAScript]
[Description("@#")]
public record VdSparkStackbarDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 当前堆叠段占总长度的比例值。
    /// </summary>
    [Description("@#proportion")]
    public double? Proportion { get; init; }

    /// <summary>
    /// 当前堆叠段的比例标签文本。
    /// </summary>
    [Description("@#proportionLabel")]
    public string? ProportionLabel { get; init; }

    /// <summary>
    /// 区间或线段的起始数值。
    /// </summary>
    [Description("@#start")]
    public double? Start { get; init; }

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double? Value { get; init; }

    /// <summary>
    /// 显示宽度；与所在组件的布局坐标或 CSS 尺寸格式保持一致。
    /// </summary>
    [Description("@#width")]
    public double? Width { get; init; }
}

/// <summary>Spark stackbar configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdSparkStackbarConfig : VdConfig;

/// <summary>Strip plot point。</summary>
[ECMAScript]
[Description("@#")]
public record VdStripPlotDatasetItem : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }
}

/// <summary>Strip plot group。</summary>
[ECMAScript]
[Description("@#")]
public record VdStripPlotDataset : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 当前条带图分类中的散点数据。
    /// </summary>
    [Description("@#plots")]
    public VdStripPlotDatasetItem[] Plots { get; init; } = [];
}

/// <summary>Strip plot configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdStripPlotConfig : VdConfig;

/// <summary>Thermometer gradient colors。</summary>
[ECMAScript]
[Description("@#")]
public record VdThermometerColors : Vue.VueProps
{
    /// <summary>
    /// 温度渐变起始端的颜色。
    /// </summary>
    [Description("@#from")]
    public string? From { get; init; }

    /// <summary>
    /// 温度渐变结束端的颜色。
    /// </summary>
    [Description("@#to")]
    public string? To { get; init; }
}

/// <summary>Thermometer dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdThermometerDataset : Vue.VueProps
{
    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 当前区间的起始数值。
    /// </summary>
    [Description("@#from")]
    public double From { get; init; }

    /// <summary>
    /// 当前区间的结束数值。
    /// </summary>
    [Description("@#to")]
    public double To { get; init; }

    /// <summary>
    /// 温度计刻度的分段设置。
    /// </summary>
    [Description("@#steps")]
    public double? Steps { get; init; }

    /// <summary>
    /// 温度计颜色渐变的起止配置。
    /// </summary>
    [Description("@#colors")]
    public VdThermometerColors? Colors { get; init; }
}

/// <summary>Thermometer configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdThermometerConfig : VdConfig;

/// <summary>Timer configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdTimerConfig : VdConfig;

/// <summary>Tiremarks dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdTiremarksDataset : Vue.VueProps
{
    /// <summary>
    /// 用于绘制百分比进度的数值，通常以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public double Percentage { get; init; }
}

/// <summary>Tiremarks configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdTiremarksConfig : VdConfig;

/// <summary>Wheel dataset root。</summary>
[ECMAScript]
[Description("@#")]
public record VdWheelDataset : Vue.VueProps
{
    /// <summary>
    /// 用于绘制百分比进度的数值，通常以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    public double Percentage { get; init; }
}

/// <summary>Wheel configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdWheelConfig : VdConfig;

/// <summary>World map country value。</summary>
[ECMAScript]
[Description("@#")]
public record VdWorldDatasetItem : Vue.VueProps
{
    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 用于对世界地图数据分组的分类名称。
    /// </summary>
    [Description("@#category")]
    public string? Category { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>World map's ISO-keyed dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VdWorldDataset : Vue.VueDictionary<VdWorldDatasetItem>;

/// <summary>World map configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdWorldConfig : VdConfig;

/// <summary>XY canvas series row。</summary>
[ECMAScript]
[Description("@#")]
public record VdXyCanvasDatasetItem : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public double?[] Series { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 此序列的图形绘制方式，具体选项见枚举成员说明。
    /// </summary>
    [Description("@#type")]
    public VdXySeriesType? Type { get; init; }

    /// <summary>
    /// 在此折线序列与基线之间绘制填充区域。
    /// </summary>
    [Description("@#useArea")]
    public bool? UseArea { get; init; }

    /// <summary>
    /// 是否显示此序列各数据点的数值标签。
    /// </summary>
    [Description("@#dataLabels")]
    public bool? DataLabels { get; init; }

    /// <summary>
    /// 此序列 Y 轴刻度的分段设置。
    /// </summary>
    [Description("@#scaleSteps")]
    public double? ScaleSteps { get; init; }

    /// <summary>
    /// 显示数值时添加的文本前缀，不改变原始数据值。
    /// </summary>
    [Description("@#prefix")]
    public string? Prefix { get; init; }

    /// <summary>
    /// 显示数值时添加的文本后缀，不改变原始数据值。
    /// </summary>
    [Description("@#suffix")]
    public string? Suffix { get; init; }

    /// <summary>
    /// 此序列数值标签保留的小数位数。
    /// </summary>
    [Description("@#rounding")]
    public double? Rounding { get; init; }

    /// <summary>
    /// 根据此序列的数据范围自动计算坐标轴范围。
    /// </summary>
    [Description("@#autoScaling")]
    public bool? AutoScaling { get; init; }

    /// <summary>
    /// 为此序列指定坐标轴下界。
    /// </summary>
    [Description("@#scaleMin")]
    public double? ScaleMin { get; init; }

    /// <summary>
    /// 为此序列指定坐标轴上界。
    /// </summary>
    [Description("@#scaleMax")]
    public double? ScaleMax { get; init; }

    /// <summary>
    /// 显示此序列对应的 Y 轴位置指示。
    /// </summary>
    [Description("@#showYMarker")]
    public bool? ShowYMarker { get; init; }
}

/// <summary>XY canvas configuration。</summary>
[ECMAScript]
[Description("@#")]
public record VdXyCanvasConfig : VdConfig;

/// <summary>
/// vue-data-ui SVG pattern 的 closed literal domain。Description 保留 upstream 的 kebab-case
/// runtime token，避免 C# identifier 命名影响 emitted JavaScript。
/// </summary>
[String]
public enum VdPatternName
{
    /// <summary>
    /// 气泡填充图案；上游标识为 “bubbles”。
    /// </summary>
    [Description("@#bubbles")]
    Bubbles,

    /// <summary>
    /// 地砖填充图案；上游标识为 “flooring”。
    /// </summary>
    [Description("@#flooring")]
    Flooring,

    /// <summary>
    /// 网格填充图案；上游标识为 “grid”。
    /// </summary>
    [Description("@#grid")]
    Grid,

    /// <summary>
    /// 六边形与菱形填充图案；上游标识为 “hexagon-diamond”。
    /// </summary>
    [Description("@#hexagon-diamond")]
    HexagonDiamond,

    /// <summary>
    /// 六边形铺砖填充图案；上游标识为 “hexagon-flooring”。
    /// </summary>
    [Description("@#hexagon-flooring")]
    HexagonFlooring,

    /// <summary>
    /// 六边形网格填充图案；上游标识为 “hexagon-grid”。
    /// </summary>
    [Description("@#hexagon-grid")]
    HexagonGrid,

    /// <summary>
    /// 迷宫填充图案；上游标识为 “maze”。
    /// </summary>
    [Description("@#maze")]
    Maze,

    /// <summary>
    /// 交错几何纹样填充图案；上游标识为 “redrum”。
    /// </summary>
    [Description("@#redrum")]
    Redrum,

    /// <summary>
    /// 鳞片填充图案；上游标识为 “scales”。
    /// </summary>
    [Description("@#scales")]
    Scales,

    /// <summary>
    /// 方格填充图案；上游标识为 “squares”。
    /// </summary>
    [Description("@#squares")]
    Squares,

    /// <summary>
    /// 波浪填充图案；上游标识为 “wave”。
    /// </summary>
    [Description("@#wave")]
    Wave,

    /// <summary>
    /// 锯齿填充图案；上游标识为 “zig-zag”。
    /// </summary>
    [Description("@#zig-zag")]
    ZigZag
}

/// <summary>
/// vue-data-ui 内置 icon 的 closed literal domain。成员与 3.23.4 declaration 对齐；运行时
/// token 由 Description 固定，新增上游 icon 时应在 catalog parity 测试更新时同步补齐。
/// </summary>
[String]
public enum VdIconName
{
    /// <summary>
    /// 无障碍图标；上游标识为 “accessibility”。
    /// </summary>
    [Description("@#accessibility")]
    Accessibility,

    /// <summary>
    /// 添加列图标；上游标识为 “addColumn”。
    /// </summary>
    [Description("@#addColumn")]
    AddColumn,

    /// <summary>
    /// 添加行图标；上游标识为 “addRow”。
    /// </summary>
    [Description("@#addRow")]
    AddRow,

    /// <summary>
    /// 字母升序图标；上游标识为 “aToZ”。
    /// </summary>
    [Description("@#aToZ")]
    AToZ,

    /// <summary>
    /// 折叠面板图标；上游标识为 “accordion”。
    /// </summary>
    [Description("@#accordion")]
    Accordion,

    /// <summary>
    /// 注释图标；上游标识为 “annotation”。
    /// </summary>
    [Description("@#annotation")]
    Annotation,

    /// <summary>
    /// 绘图标注图标；上游标识为 “annotator”。
    /// </summary>
    [Description("@#annotator")]
    Annotator,

    /// <summary>
    /// 禁用绘图标注图标；上游标识为 “annotatorDisabled”。
    /// </summary>
    [Description("@#annotatorDisabled")]
    AnnotatorDisabled,

    /// <summary>
    /// API 数据流图标；上游标识为 “apiStream”。
    /// </summary>
    [Description("@#apiStream")]
    ApiStream,

    /// <summary>
    /// 向下箭头图标；上游标识为 “arrowBottom”。
    /// </summary>
    [Description("@#arrowBottom")]
    ArrowBottom,

    /// <summary>
    /// 向左箭头图标；上游标识为 “arrowLeft”。
    /// </summary>
    [Description("@#arrowLeft")]
    ArrowLeft,

    /// <summary>
    /// 向右箭头图标；上游标识为 “arrowRight”。
    /// </summary>
    [Description("@#arrowRight")]
    ArrowRight,

    /// <summary>
    /// 向上箭头图标；上游标识为 “arrowTop”。
    /// </summary>
    [Description("@#arrowTop")]
    ArrowTop,

    /// <summary>
    /// 电池图标；上游标识为 “battery”。
    /// </summary>
    [Description("@#battery")]
    Battery,

    /// <summary>
    /// 铃铛图标；上游标识为 “bell”。
    /// </summary>
    [Description("@#bell")]
    Bell,

    /// <summary>
    /// 关闭通知图标；上游标识为 “bellOff”。
    /// </summary>
    [Description("@#bellOff")]
    BellOff,

    /// <summary>
    /// 响铃图标；上游标识为 “bellRing”。
    /// </summary>
    [Description("@#bellRing")]
    BellRing,

    /// <summary>
    /// 二进制图标；上游标识为 “binary”。
    /// </summary>
    [Description("@#binary")]
    Binary,

    /// <summary>
    /// 模糊图标；上游标识为 “blur”。
    /// </summary>
    [Description("@#blur")]
    Blur,

    /// <summary>
    /// 多个盒子图标；上游标识为 “boxes”。
    /// </summary>
    [Description("@#boxes")]
    Boxes,

    /// <summary>
    /// 根节点包含的分支数据。
    /// </summary>
    [Description("@#branches")]
    Branches,

    /// <summary>
    /// 置于底层图标；上游标识为 “bringToBack”。
    /// </summary>
    [Description("@#bringToBack")]
    BringToBack,

    /// <summary>
    /// 置于顶层图标；上游标识为 “bringToFront”。
    /// </summary>
    [Description("@#bringToFront")]
    BringToFront,

    /// <summary>
    /// 桶图标；上游标识为 “bucket”。
    /// </summary>
    [Description("@#bucket")]
    Bucket,

    /// <summary>
    /// 空桶图标；上游标识为 “bucketEmpty”。
    /// </summary>
    [Description("@#bucketEmpty")]
    BucketEmpty,

    /// <summary>
    /// 装满的桶图标；上游标识为 “bucketFill”。
    /// </summary>
    [Description("@#bucketFill")]
    BucketFill,

    /// <summary>
    /// 回收桶图标；上游标识为 “bucketRecycle”。
    /// </summary>
    [Description("@#bucketRecycle")]
    BucketRecycle,

    /// <summary>
    /// 缺陷图标；上游标识为 “bug”。
    /// </summary>
    [Description("@#bug")]
    Bug,

    /// <summary>
    /// 建筑图标；上游标识为 “building”。
    /// </summary>
    [Description("@#building")]
    Building,

    /// <summary>
    /// 日历图标；上游标识为 “calendar”。
    /// </summary>
    [Description("@#calendar")]
    Calendar,

    /// <summary>
    /// 轮播表格图标；上游标识为 “carouselTable”。
    /// </summary>
    [Description("@#carouselTable")]
    CarouselTable,

    /// <summary>
    /// 立体条形图图标；上游标识为 “chart3dBar”。
    /// </summary>
    [Description("@#chart3dBar")]
    Chart3dBar,

    /// <summary>
    /// 人口金字塔图图标；上游标识为 “chartAgePyramid”。
    /// </summary>
    [Description("@#chartAgePyramid")]
    ChartAgePyramid,

    /// <summary>
    /// 条形图图标；上游标识为 “chartBar”。
    /// </summary>
    [Description("@#chartBar")]
    ChartBar,

    /// <summary>
    /// 子弹图图标；上游标识为 “chartBullet”。
    /// </summary>
    [Description("@#chartBullet")]
    ChartBullet,

    /// <summary>
    /// 排名变化图图标；上游标识为 “chartBump”。
    /// </summary>
    [Description("@#chartBump")]
    ChartBump,

    /// <summary>
    /// 蜡烛图图标；上游标识为 “chartCandlestick”。
    /// </summary>
    [Description("@#chartCandlestick")]
    ChartCandlestick,

    /// <summary>
    /// 栗形层级图图标；上游标识为 “chartChestnut”。
    /// </summary>
    [Description("@#chartChestnut")]
    ChartChestnut,

    /// <summary>
    /// 弦图图标；上游标识为 “chartChord”。
    /// </summary>
    [Description("@#chartChord")]
    ChartChord,

    /// <summary>
    /// 圆形打包图图标；上游标识为 “chartCirclePack”。
    /// </summary>
    [Description("@#chartCirclePack")]
    ChartCirclePack,

    /// <summary>
    /// 聚类图图标；上游标识为 “chartCluster”。
    /// </summary>
    [Description("@#chartCluster")]
    ChartCluster,

    /// <summary>
    /// 有向无环图图标；上游标识为 “chartDag”。
    /// </summary>
    [Description("@#chartDag")]
    ChartDag,

    /// <summary>
    /// 环形图图标；上游标识为 “chartDonut”。
    /// </summary>
    [Description("@#chartDonut")]
    ChartDonut,

    /// <summary>
    /// 环形变化图图标；上游标识为 “chartDonutEvolution”。
    /// </summary>
    [Description("@#chartDonutEvolution")]
    ChartDonutEvolution,

    /// <summary>
    /// 哑铃图图标；上游标识为 “chartDumbbell”。
    /// </summary>
    [Description("@#chartDumbbell")]
    ChartDumbbell,

    /// <summary>
    /// 流向图图标；上游标识为 “chartFlow”。
    /// </summary>
    [Description("@#chartFlow")]
    ChartFlow,

    /// <summary>
    /// 漏斗图图标；上游标识为 “chartFunnel”。
    /// </summary>
    [Description("@#chartFunnel")]
    ChartFunnel,

    /// <summary>
    /// 星系关系图图标；上游标识为 “chartGalaxy”。
    /// </summary>
    [Description("@#chartGalaxy")]
    ChartGalaxy,

    /// <summary>
    /// 仪表盘图标；上游标识为 “chartGauge”。
    /// </summary>
    [Description("@#chartGauge")]
    ChartGauge,

    /// <summary>
    /// 热力图图标；上游标识为 “chartHeatmap”。
    /// </summary>
    [Description("@#chartHeatmap")]
    ChartHeatmap,

    /// <summary>
    /// 山丘图图标；上游标识为 “chartHill”。
    /// </summary>
    [Description("@#chartHill")]
    ChartHill,

    /// <summary>
    /// 历史趋势图图标；上游标识为 “chartHistoryPlot”。
    /// </summary>
    [Description("@#chartHistoryPlot")]
    ChartHistoryPlot,

    /// <summary>
    /// 折线图图标；上游标识为 “chartLine”。
    /// </summary>
    [Description("@#chartLine")]
    ChartLine,

    /// <summary>
    /// 心情雷达图图标；上游标识为 “chartMoodRadar”。
    /// </summary>
    [Description("@#chartMoodRadar")]
    ChartMoodRadar,

    /// <summary>
    /// 嵌套环形图图标；上游标识为 “chartNestedDonuts”。
    /// </summary>
    [Description("@#chartNestedDonuts")]
    ChartNestedDonuts,

    /// <summary>
    /// 同心进度图图标；上游标识为 “chartOnion”。
    /// </summary>
    [Description("@#chartOnion")]
    ChartOnion,

    /// <summary>
    /// 平行坐标图图标；上游标识为 “chartParallelCoordinatePlot”。
    /// </summary>
    [Description("@#chartParallelCoordinatePlot")]
    ChartParallelCoordinatePlot,

    /// <summary>
    /// 象限图图标；上游标识为 “chartQuadrant”。
    /// </summary>
    [Description("@#chartQuadrant")]
    ChartQuadrant,

    /// <summary>
    /// 雷达图图标；上游标识为 “chartRadar”。
    /// </summary>
    [Description("@#chartRadar")]
    ChartRadar,

    /// <summary>
    /// 圆形关系图图标；上游标识为 “chartRelationCircle”。
    /// </summary>
    [Description("@#chartRelationCircle")]
    ChartRelationCircle,

    /// <summary>
    /// 山脊分布图图标；上游标识为 “chartRidgeline”。
    /// </summary>
    [Description("@#chartRidgeline")]
    ChartRidgeline,

    /// <summary>
    /// 圆环图图标；上游标识为 “chartRings”。
    /// </summary>
    [Description("@#chartRings")]
    ChartRings,

    /// <summary>
    /// 散点图图标；上游标识为 “chartScatter”。
    /// </summary>
    [Description("@#chartScatter")]
    ChartScatter,

    /// <summary>
    /// 迷你直方图图标；上游标识为 “chartSparkHistogram”。
    /// </summary>
    [Description("@#chartSparkHistogram")]
    ChartSparkHistogram,

    /// <summary>
    /// 迷你堆叠条形图图标；上游标识为 “chartSparkStackbar”。
    /// </summary>
    [Description("@#chartSparkStackbar")]
    ChartSparkStackbar,

    /// <summary>
    /// 迷你条形图图标；上游标识为 “chartSparkbar”。
    /// </summary>
    [Description("@#chartSparkbar")]
    ChartSparkbar,

    /// <summary>
    /// 迷你折线图图标；上游标识为 “chartSparkline”。
    /// </summary>
    [Description("@#chartSparkline")]
    ChartSparkline,

    /// <summary>
    /// 堆叠条形图图标；上游标识为 “chartStackbar”。
    /// </summary>
    [Description("@#chartStackbar")]
    ChartStackbar,

    /// <summary>
    /// 堆叠折线图图标；上游标识为 “chartStackline”。
    /// </summary>
    [Description("@#chartStackline")]
    ChartStackline,

    /// <summary>
    /// 条带散点图图标；上游标识为 “chartStripPlot”。
    /// </summary>
    [Description("@#chartStripPlot")]
    ChartStripPlot,

    /// <summary>
    /// 数据表图标；上游标识为 “chartTable”。
    /// </summary>
    [Description("@#chartTable")]
    ChartTable,

    /// <summary>
    /// 带趋势线的数据表图标；上游标识为 “chartTableSparkline”。
    /// </summary>
    [Description("@#chartTableSparkline")]
    ChartTableSparkline,

    /// <summary>
    /// 温度计图标；上游标识为 “chartThermometer”。
    /// </summary>
    [Description("@#chartThermometer")]
    ChartThermometer,

    /// <summary>
    /// 轮胎轨迹进度图图标；上游标识为 “chartTiremarks”。
    /// </summary>
    [Description("@#chartTiremarks")]
    ChartTiremarks,

    /// <summary>
    /// 竖直条形图图标；上游标识为 “chartVerticalBar”。
    /// </summary>
    [Description("@#chartVerticalBar")]
    ChartVerticalBar,

    /// <summary>
    /// 方格占比图图标；上游标识为 “chartWaffle”。
    /// </summary>
    [Description("@#chartWaffle")]
    ChartWaffle,

    /// <summary>
    /// 轮形进度图图标；上游标识为 “chartWheel”。
    /// </summary>
    [Description("@#chartWheel")]
    ChartWheel,

    /// <summary>
    /// 词云图标；上游标识为 “chartWordCloud”。
    /// </summary>
    [Description("@#chartWordCloud")]
    ChartWordCloud,

    /// <summary>
    /// 中文词云图标；上游标识为 “chartWordCloudZh”。
    /// </summary>
    [Description("@#chartWordCloudZh")]
    ChartWordCloudZh,

    /// <summary>
    /// 勾选图标；上游标识为 “check”。
    /// </summary>
    [Description("@#check")]
    Check,

    /// <summary>
    /// 检查清单图标；上游标识为 “checkList”。
    /// </summary>
    [Description("@#checkList")]
    CheckList,

    /// <summary>
    /// 芯片图标；上游标识为 “chip”。
    /// </summary>
    [Description("@#chip")]
    Chip,

    /// <summary>
    /// 人工智能芯片图标；上游标识为 “chipAi”。
    /// </summary>
    [Description("@#chipAi")]
    ChipAi,

    /// <summary>
    /// 二进制芯片图标；上游标识为 “chipBinary”。
    /// </summary>
    [Description("@#chipBinary")]
    ChipBinary,

    /// <summary>
    /// 圆形轮廓图标；上游标识为 “circle”。
    /// </summary>
    [Description("@#circle")]
    Circle,

    /// <summary>
    /// 圆形取消图标；上游标识为 “circleCancel”。
    /// </summary>
    [Description("@#circleCancel")]
    CircleCancel,

    /// <summary>
    /// 圆形勾选图标；上游标识为 “circleCheck”。
    /// </summary>
    [Description("@#circleCheck")]
    CircleCheck,

    /// <summary>
    /// 圆形感叹号图标；上游标识为 “circleExclamation”。
    /// </summary>
    [Description("@#circleExclamation")]
    CircleExclamation,

    /// <summary>
    /// 实心圆图标；上游标识为 “circleFill”。
    /// </summary>
    [Description("@#circleFill")]
    CircleFill,

    /// <summary>
    /// 圆形问号图标；上游标识为 “circleQuestion”。
    /// </summary>
    [Description("@#circleQuestion")]
    CircleQuestion,

    /// <summary>
    /// 夸张表情机器人图标；上游标识为 “clankerCrazy”。
    /// </summary>
    [Description("@#clankerCrazy")]
    ClankerCrazy,

    /// <summary>
    /// 凶恶表情机器人图标；上游标识为 “clankerNasty”。
    /// </summary>
    [Description("@#clankerNasty")]
    ClankerNasty,

    /// <summary>
    /// 回形针图标；上游标识为 “clip”。
    /// </summary>
    [Description("@#clip")]
    Clip,

    /// <summary>
    /// 剪贴板图标；上游标识为 “clipBoard”。
    /// </summary>
    [Description("@#clipBoard")]
    ClipBoard,

    /// <summary>
    /// 带条形图的剪贴板图标；上游标识为 “clipboardBar”。
    /// </summary>
    [Description("@#clipboardBar")]
    ClipboardBar,

    /// <summary>
    /// 带环形图的剪贴板图标；上游标识为 “clipboardDonut”。
    /// </summary>
    [Description("@#clipboardDonut")]
    ClipboardDonut,

    /// <summary>
    /// 带折线图的剪贴板图标；上游标识为 “clipboardLine”。
    /// </summary>
    [Description("@#clipboardLine")]
    ClipboardLine,

    /// <summary>
    /// 带变量的剪贴板图标；上游标识为 “clipboardVariable”。
    /// </summary>
    [Description("@#clipboardVariable")]
    ClipboardVariable,

    /// <summary>
    /// 关闭图标；上游标识为 “close”。
    /// </summary>
    [Description("@#close")]
    Close,

    /// <summary>
    /// 云图标；上游标识为 “cloud”。
    /// </summary>
    [Description("@#cloud")]
    Cloud,

    /// <summary>
    /// 雨云图标；上游标识为 “cloudRain”。
    /// </summary>
    [Description("@#cloudRain")]
    CloudRain,

    /// <summary>
    /// 颜色选择器图标；上游标识为 “colorPicker”。
    /// </summary>
    [Description("@#colorPicker")]
    ColorPicker,

    /// <summary>
    /// 计算机图标；上游标识为 “computer”。
    /// </summary>
    [Description("@#computer")]
    Computer,

    /// <summary>
    /// 复制图标；上游标识为 “copy”。
    /// </summary>
    [Description("@#copy")]
    Copy,

    /// <summary>
    /// Copyleft 标志图标；上游标识为 “copyLeft”。
    /// </summary>
    [Description("@#copyLeft")]
    CopyLeft,

    /// <summary>
    /// 牛角面包图标；上游标识为 “croissant”。
    /// </summary>
    [Description("@#croissant")]
    Croissant,

    /// <summary>
    /// CSV 格式图标；上游标识为 “csv”。
    /// </summary>
    [Description("@#csv")]
    Csv,

    /// <summary>
    /// 大括号图标；上游标识为 “curlyBrackets”。
    /// </summary>
    [Description("@#curlyBrackets")]
    CurlyBrackets,

    /// <summary>
    /// 展开语法图标；上游标识为 “curlySpread”。
    /// </summary>
    [Description("@#curlySpread")]
    CurlySpread,

    /// <summary>
    /// 光标图标；上游标识为 “cursor”。
    /// </summary>
    [Description("@#cursor")]
    Cursor,

    /// <summary>
    /// 仪表板图标；上游标识为 “dashboard”。
    /// </summary>
    [Description("@#dashboard")]
    Dashboard,

    /// <summary>
    /// 数据库图标；上游标识为 “database”。
    /// </summary>
    [Description("@#database")]
    Database,

    /// <summary>
    /// 菱形轮廓图标；上游标识为 “diamond”。
    /// </summary>
    [Description("@#diamond")]
    Diamond,

    /// <summary>
    /// 实心菱形图标；上游标识为 “diamondFill”。
    /// </summary>
    [Description("@#diamondFill")]
    DiamondFill,

    /// <summary>
    /// 数字 0图标；上游标识为 “digit0”。
    /// </summary>
    [Description("@#digit0")]
    Digit0,

    /// <summary>
    /// 数字 1图标；上游标识为 “digit1”。
    /// </summary>
    [Description("@#digit1")]
    Digit1,

    /// <summary>
    /// 数字 2图标；上游标识为 “digit2”。
    /// </summary>
    [Description("@#digit2")]
    Digit2,

    /// <summary>
    /// 数字 3图标；上游标识为 “digit3”。
    /// </summary>
    [Description("@#digit3")]
    Digit3,

    /// <summary>
    /// 数字 4图标；上游标识为 “digit4”。
    /// </summary>
    [Description("@#digit4")]
    Digit4,

    /// <summary>
    /// 数字 5图标；上游标识为 “digit5”。
    /// </summary>
    [Description("@#digit5")]
    Digit5,

    /// <summary>
    /// 数字 6图标；上游标识为 “digit6”。
    /// </summary>
    [Description("@#digit6")]
    Digit6,

    /// <summary>
    /// 数字 7图标；上游标识为 “digit7”。
    /// </summary>
    [Description("@#digit7")]
    Digit7,

    /// <summary>
    /// 数字 8图标；上游标识为 “digit8”。
    /// </summary>
    [Description("@#digit8")]
    Digit8,

    /// <summary>
    /// 数字 9图标；上游标识为 “digit9”。
    /// </summary>
    [Description("@#digit9")]
    Digit9,

    /// <summary>
    /// 方向图标；上游标识为 “direction”。
    /// </summary>
    [Description("@#direction")]
    Direction,

    /// <summary>
    /// 文档图标；上游标识为 “document”。
    /// </summary>
    [Description("@#document")]
    Document,

    /// <summary>
    /// 双勾图标；上游标识为 “doubleCheck”。
    /// </summary>
    [Description("@#doubleCheck")]
    DoubleCheck,

    /// <summary>
    /// 双闪光图标；上游标识为 “doubleSpark”。
    /// </summary>
    [Description("@#doubleSpark")]
    DoubleSpark,

    /// <summary>
    /// 下载图标；上游标识为 “download”。
    /// </summary>
    [Description("@#download")]
    Download,

    /// <summary>
    /// 信封图标；上游标识为 “envelope”。
    /// </summary>
    [Description("@#envelope")]
    Envelope,

    /// <summary>
    /// Excel 表格图标；上游标识为 “excel”。
    /// </summary>
    [Description("@#excel")]
    Excel,

    /// <summary>
    /// 退出全屏图标；上游标识为 “exitFullscreen”。
    /// </summary>
    [Description("@#exitFullscreen")]
    ExitFullscreen,

    /// <summary>
    /// 导出图标；上游标识为 “export”。
    /// </summary>
    [Description("@#export")]
    Export,

    /// <summary>
    /// 外部链接图标；上游标识为 “externalLink”。
    /// </summary>
    [Description("@#externalLink")]
    ExternalLink,

    /// <summary>
    /// 眼睛图标；上游标识为 “eye”。
    /// </summary>
    [Description("@#eye")]
    Eye,

    /// <summary>
    /// 文件图标；上游标识为 “file”。
    /// </summary>
    [Description("@#file")]
    File,

    /// <summary>
    /// CSV 文件图标；上游标识为 “fileCsv”。
    /// </summary>
    [Description("@#fileCsv")]
    FileCsv,

    /// <summary>
    /// PDF 文件图标；上游标识为 “filePdf”。
    /// </summary>
    [Description("@#filePdf")]
    FilePdf,

    /// <summary>
    /// 新增文件图标；上游标识为 “filePlus”。
    /// </summary>
    [Description("@#filePlus")]
    FilePlus,

    /// <summary>
    /// PNG 图片文件图标；上游标识为 “filePng”。
    /// </summary>
    [Description("@#filePng")]
    FilePng,

    /// <summary>
    /// SVG 文件图标；上游标识为 “fileSvg”。
    /// </summary>
    [Description("@#fileSvg")]
    FileSvg,

    /// <summary>
    /// 搜索文件图标；上游标识为 “fileSearch”。
    /// </summary>
    [Description("@#fileSearch")]
    FileSearch,

    /// <summary>
    /// 聚焦图标；上游标识为 “focus”。
    /// </summary>
    [Description("@#focus")]
    Focus,

    /// <summary>
    /// 文件夹轮廓图标；上游标识为 “folder”。
    /// </summary>
    [Description("@#folder")]
    Folder,

    /// <summary>
    /// 实心文件夹图标；上游标识为 “folderFill”。
    /// </summary>
    [Description("@#folderFill")]
    FolderFill,

    /// <summary>
    /// 打开的文件夹图标；上游标识为 “folderOpen”。
    /// </summary>
    [Description("@#folderOpen")]
    FolderOpen,

    /// <summary>
    /// 实心打开文件夹图标；上游标识为 “folderOpenFill”。
    /// </summary>
    [Description("@#folderOpenFill")]
    FolderOpenFill,

    /// <summary>
    /// 分叉图标；上游标识为 “fork”。
    /// </summary>
    [Description("@#fork")]
    Fork,

    /// <summary>
    /// 框线图标；上游标识为 “frameLine”。
    /// </summary>
    [Description("@#frameLine")]
    FrameLine,

    /// <summary>
    /// 全屏图标；上游标识为 “fullscreen”。
    /// </summary>
    [Description("@#fullscreen")]
    Fullscreen,

    /// <summary>
    /// 函数图标；上游标识为 “func”。
    /// </summary>
    [Description("@#func")]
    Func,

    /// <summary>
    /// GIS 图层查询图标；上游标识为 “gisLayerQuery”。
    /// </summary>
    [Description("@#gisLayerQuery")]
    GisLayerQuery,

    /// <summary>
    /// GIS 图层搜索图标；上游标识为 “gisLayerSearch”。
    /// </summary>
    [Description("@#gisLayerSearch")]
    GisLayerSearch,

    /// <summary>
    /// 六边形轮廓图标；上游标识为 “hexagon”。
    /// </summary>
    [Description("@#hexagon")]
    Hexagon,

    /// <summary>
    /// 实心六边形图标；上游标识为 “hexagonFill”。
    /// </summary>
    [Description("@#hexagonFill")]
    HexagonFill,

    /// <summary>
    /// 层级结构图标；上游标识为 “hierarchy”。
    /// </summary>
    [Description("@#hierarchy")]
    Hierarchy,

    /// <summary>
    /// 直方图图标；上游标识为 “histogram”。
    /// </summary>
    [Description("@#histogram")]
    Histogram,

    /// <summary>
    /// 下降直方图图标；上游标识为 “histogramDown”。
    /// </summary>
    [Description("@#histogramDown")]
    HistogramDown,

    /// <summary>
    /// 上升直方图图标；上游标识为 “histogramUp”。
    /// </summary>
    [Description("@#histogramUp")]
    HistogramUp,

    /// <summary>
    /// 房屋轮廓图标；上游标识为 “home”。
    /// </summary>
    [Description("@#home")]
    Home,

    /// <summary>
    /// 实心房屋图标；上游标识为 “homeFilled”。
    /// </summary>
    [Description("@#homeFilled")]
    HomeFilled,

    /// <summary>
    /// 沙漏图标；上游标识为 “hourglass”。
    /// </summary>
    [Description("@#hourglass")]
    Hourglass,

    /// <summary>
    /// HTML 标签图标；上游标识为 “htmlTag”。
    /// </summary>
    [Description("@#htmlTag")]
    HtmlTag,

    /// <summary>
    /// 图标集合图标；上游标识为 “icons”。
    /// </summary>
    [Description("@#icons")]
    Icons,

    /// <summary>
    /// 图片图标；上游标识为 “image”。
    /// </summary>
    [Description("@#image")]
    Image,

    /// <summary>
    /// 钥匙图标；上游标识为 “key”。
    /// </summary>
    [Description("@#key")]
    Key,

    /// <summary>
    /// 旋钮图标；上游标识为 “knobs”。
    /// </summary>
    [Description("@#knobs")]
    Knobs,

    /// <summary>
    /// 关键指标图标；上游标识为 “kpi”。
    /// </summary>
    [Description("@#kpi")]
    Kpi,

    /// <summary>
    /// 方框关键指标图标；上游标识为 “kpiBox”。
    /// </summary>
    [Description("@#kpiBox")]
    KpiBox,

    /// <summary>
    /// 关闭标签图标；上游标识为 “labelClose”。
    /// </summary>
    [Description("@#labelClose")]
    LabelClose,

    /// <summary>
    /// 打开标签图标；上游标识为 “labelOpen”。
    /// </summary>
    [Description("@#labelOpen")]
    LabelOpen,

    /// <summary>
    /// Lambda 符号图标；上游标识为 “lambda”。
    /// </summary>
    [Description("@#lambda")]
    Lambda,

    /// <summary>
    /// 计圈图标；上游标识为 “lap”。
    /// </summary>
    [Description("@#lap")]
    Lap,

    /// <summary>
    /// 笔记本电脑图标；上游标识为 “laptop”。
    /// </summary>
    [Description("@#laptop")]
    Laptop,

    /// <summary>
    /// 图例图标；上游标识为 “legend”。
    /// </summary>
    [Description("@#legend")]
    Legend,

    /// <summary>
    /// 熄灭的灯泡图标；上游标识为 “lightBulbOff”。
    /// </summary>
    [Description("@#lightBulbOff")]
    LightBulbOff,

    /// <summary>
    /// 点亮的灯泡图标；上游标识为 “lightBulbOn”。
    /// </summary>
    [Description("@#lightBulbOn")]
    LightBulbOn,

    /// <summary>
    /// 上升折线图标；上游标识为 “lineUp”。
    /// </summary>
    [Description("@#lineUp")]
    LineUp,

    /// <summary>
    /// 链接图标；上游标识为 “link”。
    /// </summary>
    [Description("@#link")]
    Link,

    /// <summary>
    /// 列表类型图标；上游标识为 “listType”。
    /// </summary>
    [Description("@#listType")]
    ListType,

    /// <summary>
    /// 锁定图标；上游标识为 “lock”。
    /// </summary>
    [Description("@#lock")]
    Lock,

    /// <summary>
    /// 放大镜图标；上游标识为 “magnify”。
    /// </summary>
    [Description("@#magnify")]
    Magnify,

    /// <summary>
    /// 菜单图标；上游标识为 “menu”。
    /// </summary>
    [Description("@#menu")]
    Menu,

    /// <summary>
    /// 显微镜图标；上游标识为 “microscope”。
    /// </summary>
    [Description("@#microscope")]
    Microscope,

    /// <summary>
    /// 缩略地图图标；上游标识为 “minimap”。
    /// </summary>
    [Description("@#minimap")]
    Minimap,

    /// <summary>
    /// 减号图标；上游标识为 “minus”。
    /// </summary>
    [Description("@#minus")]
    Minus,

    /// <summary>
    /// 显示器图标；上游标识为 “monitor”。
    /// </summary>
    [Description("@#monitor")]
    Monitor,

    /// <summary>
    /// 尴尬表情图标；上游标识为 “moodEmbarrassed”。
    /// </summary>
    [Description("@#moodEmbarrassed")]
    MoodEmbarrassed,

    /// <summary>
    /// 平淡表情图标；上游标识为 “moodFlat”。
    /// </summary>
    [Description("@#moodFlat")]
    MoodFlat,

    /// <summary>
    /// 开心表情图标；上游标识为 “moodHappy”。
    /// </summary>
    [Description("@#moodHappy")]
    MoodHappy,

    /// <summary>
    /// 大笑表情图标；上游标识为 “moodLaughing”。
    /// </summary>
    [Description("@#moodLaughing")]
    MoodLaughing,

    /// <summary>
    /// 中性表情图标；上游标识为 “moodNeutral”。
    /// </summary>
    [Description("@#moodNeutral")]
    MoodNeutral,

    /// <summary>
    /// 悲伤表情图标；上游标识为 “moodSad”。
    /// </summary>
    [Description("@#moodSad")]
    MoodSad,

    /// <summary>
    /// 惊讶表情图标；上游标识为 “moodSurprised”。
    /// </summary>
    [Description("@#moodSurprised")]
    MoodSurprised,

    /// <summary>
    /// 眨眼表情图标；上游标识为 “moodWink”。
    /// </summary>
    [Description("@#moodWink")]
    MoodWink,

    /// <summary>
    /// 移动图标；上游标识为 “move”。
    /// </summary>
    [Description("@#move")]
    Move,

    /// <summary>
    /// Mu 符号图标；上游标识为 “mu”。
    /// </summary>
    [Description("@#mu")]
    Mu,

    /// <summary>
    /// 网络图标；上游标识为 “network”。
    /// </summary>
    [Description("@#network")]
    Network,

    /// <summary>
    /// 数字降序图标；上游标识为 “nineToZero”。
    /// </summary>
    [Description("@#nineToZero")]
    NineToZero,

    /// <summary>
    /// npmx 标志图标；上游标识为 “npmx”。
    /// </summary>
    [Description("@#npmx")]
    Npmx,

    /// <summary>
    /// 数字图标；上游标识为 “numbers”。
    /// </summary>
    [Description("@#numbers")]
    Numbers,

    /// <summary>
    /// 调色板图标；上游标识为 “palette”。
    /// </summary>
    [Description("@#palette")]
    Palette,

    /// <summary>
    /// 暂停图标；上游标识为 “pause”。
    /// </summary>
    [Description("@#pause")]
    Pause,

    /// <summary>
    /// PDF 格式图标；上游标识为 “pdf”。
    /// </summary>
    [Description("@#pdf")]
    Pdf,

    /// <summary>
    /// 五边形轮廓图标；上游标识为 “pentagon”。
    /// </summary>
    [Description("@#pentagon")]
    Pentagon,

    /// <summary>
    /// 实心五边形图标；上游标识为 “pentagonFill”。
    /// </summary>
    [Description("@#pentagonFill")]
    PentagonFill,

    /// <summary>
    /// 多人图标；上游标识为 “people”。
    /// </summary>
    [Description("@#people")]
    People,

    /// <summary>
    /// 用于绘制百分比进度的数值，通常以 0 到 100 表示。
    /// </summary>
    [Description("@#percentage")]
    Percentage,

    /// <summary>
    /// 百分比下降图标；上游标识为 “percentageDown”。
    /// </summary>
    [Description("@#percentageDown")]
    PercentageDown,

    /// <summary>
    /// 百分比上升图标；上游标识为 “percentageUp”。
    /// </summary>
    [Description("@#percentageUp")]
    PercentageUp,

    /// <summary>
    /// 单人图标；上游标识为 “person”。
    /// </summary>
    [Description("@#person")]
    Person,

    /// <summary>
    /// 圆周率符号图标；上游标识为 “pi”。
    /// </summary>
    [Description("@#pi")]
    Pi,

    /// <summary>
    /// 饼图图标；上游标识为 “pie”。
    /// </summary>
    [Description("@#pie")]
    Pie,

    /// <summary>
    /// 播放图标；上游标识为 “play”。
    /// </summary>
    [Description("@#play")]
    Play,

    /// <summary>
    /// 图中箭头图标；上游标识为 “plotArrow”。
    /// </summary>
    [Description("@#plotArrow")]
    PlotArrow,

    /// <summary>
    /// 图中线段图标；上游标识为 “plotLine”。
    /// </summary>
    [Description("@#plotLine")]
    PlotLine,

    /// <summary>
    /// 插头图标；上游标识为 “plug”。
    /// </summary>
    [Description("@#plug")]
    Plug,

    /// <summary>
    /// 加号图标；上游标识为 “plus”。
    /// </summary>
    [Description("@#plus")]
    Plus,

    /// <summary>
    /// 指针图标；上游标识为 “pointer”。
    /// </summary>
    [Description("@#pointer")]
    Pointer,

    /// <summary>
    /// 打印机图标；上游标识为 “printer”。
    /// </summary>
    [Description("@#printer")]
    Printer,

    /// <summary>
    /// 拼图轮廓图标；上游标识为 “puzzle”。
    /// </summary>
    [Description("@#puzzle")]
    Puzzle,

    /// <summary>
    /// 实心拼图图标；上游标识为 “puzzleFill”。
    /// </summary>
    [Description("@#puzzleFill")]
    PuzzleFill,

    /// <summary>
    /// 比例图标；上游标识为 “ratio”。
    /// </summary>
    [Description("@#ratio")]
    Ratio,

    /// <summary>
    /// 回收图标；上游标识为 “recycle”。
    /// </summary>
    [Description("@#recycle")]
    Recycle,

    /// <summary>
    /// 刷新图标；上游标识为 “refresh”。
    /// </summary>
    [Description("@#refresh")]
    Refresh,

    /// <summary>
    /// 调整大小图标；上游标识为 “resize”。
    /// </summary>
    [Description("@#resize")]
    Resize,

    /// <summary>
    /// 左上至右下缩放图标；上游标识为 “resizeTLBR”。
    /// </summary>
    [Description("@#resizeTLBR")]
    ResizeTLBR,

    /// <summary>
    /// 右上至左下缩放图标；上游标识为 “resizeTRBL”。
    /// </summary>
    [Description("@#resizeTRBL")]
    ResizeTRBL,

    /// <summary>
    /// 水平缩放图标；上游标识为 “resizeX”。
    /// </summary>
    [Description("@#resizeX")]
    ResizeX,

    /// <summary>
    /// 垂直缩放图标；上游标识为 “resizeY”。
    /// </summary>
    [Description("@#resizeY")]
    ResizeY,

    /// <summary>
    /// 重新开始图标；上游标识为 “restart”。
    /// </summary>
    [Description("@#restart")]
    Restart,

    /// <summary>
    /// 撤回图标；上游标识为 “revert”。
    /// </summary>
    [Description("@#revert")]
    Revert,

    /// <summary>
    /// 机器人图标；上游标识为 “robot”。
    /// </summary>
    [Description("@#robot")]
    Robot,

    /// <summary>
    /// 保存图标；上游标识为 “save”。
    /// </summary>
    [Description("@#save")]
    Save,

    /// <summary>
    /// 工业监控图标；上游标识为 “scada”。
    /// </summary>
    [Description("@#scada")]
    Scada,

    /// <summary>
    /// 截图图标；上游标识为 “screenshot”。
    /// </summary>
    [Description("@#screenshot")]
    Screenshot,

    /// <summary>
    /// 选择并分组图标；上游标识为 “selectAndGroup”。
    /// </summary>
    [Description("@#selectAndGroup")]
    SelectAndGroup,

    /// <summary>
    /// 设置图标；上游标识为 “settings”。
    /// </summary>
    [Description("@#settings")]
    Settings,

    /// <summary>
    /// 盾牌图标；上游标识为 “shield”。
    /// </summary>
    [Description("@#shield")]
    Shield,

    /// <summary>
    /// 带感叹号的盾牌图标；上游标识为 “shieldExclam”。
    /// </summary>
    [Description("@#shieldExclam")]
    ShieldExclam,

    /// <summary>
    /// 求和符号图标；上游标识为 “sigma”。
    /// </summary>
    [Description("@#sigma")]
    Sigma,

    /// <summary>
    /// 骨架占位图标；上游标识为 “skeleton”。
    /// </summary>
    [Description("@#skeleton")]
    Skeleton,

    /// <summary>
    /// 滑块控制图标；上游标识为 “sliders”。
    /// </summary>
    [Description("@#sliders")]
    Sliders,

    /// <summary>
    /// 笑脸图标；上游标识为 “smiley”。
    /// </summary>
    [Description("@#smiley")]
    Smiley,

    /// <summary>
    /// 排序图标；上游标识为 “sort”。
    /// </summary>
    [Description("@#sort")]
    Sort,

    /// <summary>
    /// 闪光图标；上游标识为 “spark”。
    /// </summary>
    [Description("@#spark")]
    Spark,

    /// <summary>
    /// 旋转图标；上游标识为 “spin”。
    /// </summary>
    [Description("@#spin")]
    Spin,

    /// <summary>
    /// 加载旋转图案一图标；上游标识为 “spinner1”。
    /// </summary>
    [Description("@#spinner1")]
    Spinner1,

    /// <summary>
    /// 加载旋转图案二图标；上游标识为 “spinner2”。
    /// </summary>
    [Description("@#spinner2")]
    Spinner2,

    /// <summary>
    /// 加载旋转图案三图标；上游标识为 “spinner3”。
    /// </summary>
    [Description("@#spinner3")]
    Spinner3,

    /// <summary>
    /// 加载旋转图案四图标；上游标识为 “spinner4”。
    /// </summary>
    [Description("@#spinner4")]
    Spinner4,

    /// <summary>
    /// SQL 数据库图标；上游标识为 “sql”。
    /// </summary>
    [Description("@#sql")]
    Sql,

    /// <summary>
    /// SQL 查询图标；上游标识为 “sqlQuery”。
    /// </summary>
    [Description("@#sqlQuery")]
    SqlQuery,

    /// <summary>
    /// SQL 搜索图标；上游标识为 “sqlSearch”。
    /// </summary>
    [Description("@#sqlSearch")]
    SqlSearch,

    /// <summary>
    /// 方形轮廓图标；上游标识为 “square”。
    /// </summary>
    [Description("@#square")]
    Square,

    /// <summary>
    /// 实心方形图标；上游标识为 “squareFill”。
    /// </summary>
    [Description("@#squareFill")]
    SquareFill,

    /// <summary>
    /// 堆叠图标；上游标识为 “stack”。
    /// </summary>
    [Description("@#stack")]
    Stack,

    /// <summary>
    /// 星形轮廓图标；上游标识为 “star”。
    /// </summary>
    [Description("@#star")]
    Star,

    /// <summary>
    /// 带表情的星形图标；上游标识为 “starFace”。
    /// </summary>
    [Description("@#starFace")]
    StarFace,

    /// <summary>
    /// 实心星形图标；上游标识为 “starFill”。
    /// </summary>
    [Description("@#starFill")]
    StarFill,

    /// <summary>
    /// 停止图标；上游标识为 “stop”。
    /// </summary>
    [Description("@#stop")]
    Stop,

    /// <summary>
    /// 太阳图标；上游标识为 “sun”。
    /// </summary>
    [Description("@#sun")]
    Sun,

    /// <summary>
    /// SVG 格式图标；上游标识为 “svg”。
    /// </summary>
    [Description("@#svg")]
    Svg,

    /// <summary>
    /// 关闭表格图标；上游标识为 “tableClose”。
    /// </summary>
    [Description("@#tableClose")]
    TableClose,

    /// <summary>
    /// 关闭表格对话框图标；上游标识为 “tableDialogClose”。
    /// </summary>
    [Description("@#tableDialogClose")]
    TableDialogClose,

    /// <summary>
    /// 打开表格对话框图标；上游标识为 “tableDialogOpen”。
    /// </summary>
    [Description("@#tableDialogOpen")]
    TableDialogOpen,

    /// <summary>
    /// 打开表格图标；上游标识为 “tableOpen”。
    /// </summary>
    [Description("@#tableOpen")]
    TableOpen,

    /// <summary>
    /// 标签图标；上游标识为 “tag”。
    /// </summary>
    [Description("@#tag")]
    Tag,

    /// <summary>
    /// 用于对比实际值的目标数值。
    /// </summary>
    [Description("@#target")]
    Target,

    /// <summary>
    /// 测试图标；上游标识为 “test”。
    /// </summary>
    [Description("@#test")]
    Test,

    /// <summary>
    /// 在当前标题或副标题位置显示的文本。
    /// </summary>
    [Description("@#text")]
    Text,

    /// <summary>
    /// 提示气泡图标；上游标识为 “tooltip”。
    /// </summary>
    [Description("@#tooltip")]
    Tooltip,

    /// <summary>
    /// 禁用提示气泡图标；上游标识为 “tooltipDisabled”。
    /// </summary>
    [Description("@#tooltipDisabled")]
    TooltipDisabled,

    /// <summary>
    /// 垃圾桶图标；上游标识为 “trash”。
    /// </summary>
    [Description("@#trash")]
    Trash,

    /// <summary>
    /// 趋势图标；上游标识为 “trend”。
    /// </summary>
    [Description("@#trend")]
    Trend,

    /// <summary>
    /// 下降趋势图标；上游标识为 “trendDown”。
    /// </summary>
    [Description("@#trendDown")]
    TrendDown,

    /// <summary>
    /// 上升趋势图标；上游标识为 “trendUp”。
    /// </summary>
    [Description("@#trendUp")]
    TrendUp,

    /// <summary>
    /// 三角形轮廓图标；上游标识为 “triangle”。
    /// </summary>
    [Description("@#triangle")]
    Triangle,

    /// <summary>
    /// 三角警告图标；上游标识为 “triangleExclamation”。
    /// </summary>
    [Description("@#triangleExclamation")]
    TriangleExclamation,

    /// <summary>
    /// 实心三角形图标；上游标识为 “triangleFill”。
    /// </summary>
    [Description("@#triangleFill")]
    TriangleFill,

    /// <summary>
    /// 三角信息图标；上游标识为 “triangleInformation”。
    /// </summary>
    [Description("@#triangleInformation")]
    TriangleInformation,

    /// <summary>
    /// 枝条图标；上游标识为 “twig”。
    /// </summary>
    [Description("@#twig")]
    Twig,

    /// <summary>
    /// 解除锁定图标；上游标识为 “unlock”。
    /// </summary>
    [Description("@#unlock")]
    Unlock,

    /// <summary>
    /// 拔出插头图标；上游标识为 “unplug”。
    /// </summary>
    [Description("@#unplug")]
    Unplug,

    /// <summary>
    /// 取消堆叠图标；上游标识为 “unstack”。
    /// </summary>
    [Description("@#unstack")]
    Unstack,

    /// <summary>
    /// 上传图标；上游标识为 “upload”。
    /// </summary>
    [Description("@#upload")]
    Upload,

    /// <summary>
    /// Vue Data UI 标志图标；上游标识为 “vueDataUi”。
    /// </summary>
    [Description("@#vueDataUi")]
    VueDataUi,

    /// <summary>
    /// 无线网络图标；上游标识为 “wifi”。
    /// </summary>
    [Description("@#wifi")]
    Wifi,

    /// <summary>
    /// 世界地图图标；上游标识为 “world”。
    /// </summary>
    [Description("@#world")]
    World,

    /// <summary>
    /// 扳手图标；上游标识为 “wrench”。
    /// </summary>
    [Description("@#wrench")]
    Wrench,

    /// <summary>
    /// 字母降序图标；上游标识为 “zToA”。
    /// </summary>
    [Description("@#zToA")]
    ZToA,

    /// <summary>
    /// 数字升序图标；上游标识为 “zeroToNine”。
    /// </summary>
    [Description("@#zeroToNine")]
    ZeroToNine,

    /// <summary>
    /// 锁定缩放图标；上游标识为 “zoomLock”。
    /// </summary>
    [Description("@#zoomLock")]
    ZoomLock,

    /// <summary>
    /// 缩小图标；上游标识为 “zoomMinus”。
    /// </summary>
    [Description("@#zoomMinus")]
    ZoomMinus,

    /// <summary>
    /// 放大图标；上游标识为 “zoomPlus”。
    /// </summary>
    [Description("@#zoomPlus")]
    ZoomPlus,

    /// <summary>
    /// 解除缩放锁定图标；上游标识为 “zoomUnlock”。
    /// </summary>
    [Description("@#zoomUnlock")]
    ZoomUnlock
}