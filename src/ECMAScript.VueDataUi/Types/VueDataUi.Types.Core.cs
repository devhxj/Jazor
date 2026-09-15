namespace ECMAScript.VueDataUi;

/// <summary>
/// vue-data-ui 图表配置的公共基础形状。它仍是普通 JavaScript object；继承字典是为了让上游
/// 持续新增的 nested option 能以 <see cref="Vue.VueProps"/> 扩展，而不退回到 <see cref="object"/>。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiConfig : Vue.VueDictionary<Vue.VueValue>
{
    /// <summary>启用 upstream debug 输出。Enables upstream debug output.</summary>
    [Description("@#debug")]
    public bool? Debug { get; init; }

    /// <summary>显示 skeleton/loading 状态。Shows the upstream loading state.</summary>
    [Description("@#loading")]
    public bool? Loading { get; init; }

    /// <summary>启用容器尺寸响应。Responsive charts need a bounded container height.</summary>
    [Description("@#responsive")]
    public bool? Responsive { get; init; }

    /// <summary>库内置主题。The built-in chart theme.</summary>
    [Description("@#theme")]
    public VueDataUiTheme? Theme { get; init; }

    /// <summary>替换默认 palette 的颜色列表。Overrides the default color palette.</summary>
    [Description("@#customPalette")]
    public string[]? CustomPalette { get; init; }

    /// <summary>开启 CSS animation。Enables the library CSS animation path.</summary>
    [Description("@#useCssAnimation")]
    public bool? UseCssAnimation { get; init; }

    /// <summary>为可点击 datapoint 显示 pointer cursor。Opt-in pointer cursor for interactive data points.</summary>
    [Description("@#useCursorPointer")]
    public bool? UseCursorPointer { get; init; }
}

/// <summary>vue-data-ui 内置主题 literal。</summary>
[String]
public enum VueDataUiTheme
{
    /// <summary>
    /// 浅色外观；上游取值为 “light”。
    /// </summary>
    [Description("@#light")]
    Light,

    /// <summary>
    /// 深色外观；上游取值为 “dark”。
    /// </summary>
    [Description("@#dark")]
    Dark
}

/// <summary>
/// 可扩展 dataset item 的基类。复杂图表可派生并声明其稳定字段，未知上游字段通过
/// collection initializer 以 <see cref="Vue.VueValue"/> 写入，仍会 lowering 为 plain object。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiDatasetItem : Vue.VueDictionary<Vue.VueValue>;

/// <summary>通用 downsample 选项，适合 XY、scatter 与 sparkline 的大数据量场景。</summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiDownsampleOptions : Vue.VueProps
{
    /// <summary>
    /// 降采样后目标数据点数量的阈值。
    /// </summary>
    [Description("@#threshold")]
    public int? Threshold { get; init; }
}

/// <summary>通用 chart 标题形状。Common title shape shared by the major chart families.</summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiChartTitle : Vue.VueProps
{
    /// <summary>
    /// 在当前标题或副标题位置显示的文本。
    /// </summary>
    [Description("@#text")]
    public string? Text { get; init; }

    /// <summary>
    /// 主标题下方的副标题文本与样式配置。
    /// </summary>
    [Description("@#subtitle")]
    public VueDataUiChartSubtitle? Subtitle { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 标题文字的字号。
    /// </summary>
    [Description("@#fontSize")]
    public double? FontSize { get; init; }

    /// <summary>
    /// 是否使用粗体显示标题。
    /// </summary>
    [Description("@#bold")]
    public bool? Bold { get; init; }
}

/// <summary>通用 chart subtitle 形状。</summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiChartSubtitle : Vue.VueProps
{
    /// <summary>
    /// 在当前标题或副标题位置显示的文本。
    /// </summary>
    [Description("@#text")]
    public string? Text { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>
/// 表格与 candlestick tuple 中的单元值。上游只接受 string 或 number，native union 保持该边界，
/// 不把表格数据放宽成 <c>object[]</c>。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VueDataUiCellValue(string, double)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 double 分支；不属于该分支时返回 null。
    /// </summary>
    public double? AsNumber => Value is double value ? value : default(double?);
}

/// <summary>XY 图表 series 类型。</summary>
[String]
public enum VueUiXySeriesType
{
    /// <summary>
    /// 以条形绘制数据；上游取值为 “bar”。
    /// </summary>
    [Description("@#bar")]
    Bar,

    /// <summary>
    /// 线条形式；上游取值为 “line”。
    /// </summary>
    [Description("@#line")]
    Line,

    /// <summary>
    /// 仅绘制数据点，适用于散点展示。
    /// </summary>
    [Description("@#plot")]
    Plot
}

/// <summary>XY coordinate series 的一个点。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyCoordinate : Vue.VueProps
{
    /// <summary>
    /// 数据点在 X 轴上的数值坐标。
    /// </summary>
    [Description("@#x")]
    public double? X { get; init; }

    /// <summary>
    /// 数据点在 Y 轴上的数值坐标。
    /// </summary>
    [Description("@#y")]
    public double? Y { get; init; }
}

/// <summary>XY series 可为 sequential values 或显式 coordinates。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUiXySeriesValues(double?[], VueUiXyCoordinate[])
{
    /// <summary>
    /// 读取当前值的 double?[] 分支；不属于该分支时返回 null。
    /// </summary>
    public double?[]? AsValues => Value as double?[];

    /// <summary>
    /// 读取当前值的 VueUiXyCoordinate[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueUiXyCoordinate[]? AsCoordinates => Value as VueUiXyCoordinate[];
}

/// <summary>VueUiXy 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyDatasetItem : VueDataUiDatasetItem
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
    public VueUiXySeriesValues Series { get; init; } = default!;

    /// <summary>
    /// 此序列的图形绘制方式，具体选项见枚举成员说明。
    /// </summary>
    [Description("@#type")]
    public VueUiXySeriesType Type { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 在此折线序列与基线之间绘制填充区域。
    /// </summary>
    [Description("@#useArea")]
    public bool? UseArea { get; init; }

    /// <summary>
    /// 使用平滑曲线连接序列中的相邻数据点。
    /// </summary>
    [Description("@#smooth")]
    public bool? Smooth { get; init; }

    /// <summary>
    /// 是否显示此序列各数据点的数值标签。
    /// </summary>
    [Description("@#dataLabels")]
    public bool? DataLabels { get; init; }
}

/// <summary>XY config 的稳定公共字段；详细 layout 继续可通过基类字典加 record 扩展。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyConfig : VueDataUiConfig
{
    /// <summary>
    /// 大数据量时的降采样配置，可设置保留数据点的阈值。
    /// </summary>
    [Description("@#downsample")]
    public VueDataUiDownsampleOptions? Downsample { get; init; }

    /// <summary>
    /// 启用面向大数据量的性能模式；组件将使用其性能模式的绘制策略。
    /// </summary>
    [Description("@#usePerformanceMode")]
    public bool? UsePerformanceMode { get; init; }
}

/// <summary>VueUiDonut 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutDatasetItem : VueDataUiDatasetItem
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
    public double[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 附加到此环形图项的说明文本，可用于注释和提示展示。
    /// </summary>
    [Description("@#comment")]
    public string? Comment { get; init; }
}

/// <summary>
/// VueUiDonut <c>selectLegend</c> event 的图例摘要。它是 runtime emitted shape，
/// 与 authoring dataset 分开，避免把 <c>values</c> 误当成单一聚合值。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutLegendItem : Vue.VueProps
{
    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string Color { get; init; } = string.Empty;

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

/// <summary>Donut 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutConfig : VueDataUiConfig;

/// <summary>Gauge 的一个 range series。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGaugeDatasetSerieItem : Vue.VueProps
{
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
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string? Name { get; init; }
}

/// <summary>VueUiGauge 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGaugeDataset : Vue.VueProps
{
    /// <summary>
    /// 仪表盘的基准数值。
    /// </summary>
    [Description("@#base")]
    public double? Base { get; init; }

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VueUiGaugeDatasetSerieItem[] Series { get; init; } = [];

    /// <summary>
    /// 数据项的唯一标识；应在当前集合内保持稳定。
    /// </summary>
    [Description("@#id")]
    public Vue.VueStringNumberValue? Id { get; init; }
}

/// <summary>Gauge 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGaugeConfig : VueDataUiConfig;

/// <summary>Vertical/Horizontal bar children。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiVerticalBarDatasetChild : Vue.VueProps
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
    public double? Value { get; init; }
}

/// <summary>Vertical/Horizontal bar 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiVerticalBarDatasetItem : VueDataUiDatasetItem
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
    public double? Value { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 当前数据项的子节点，用于形成层级或分类细分。
    /// </summary>
    [Description("@#children")]
    public VueUiVerticalBarDatasetChild[]? Children { get; init; }
}

/// <summary>
/// Horizontal Bar 的 C# authoring name。upstream 是 Vertical Bar row 的 type alias，
/// 这里保留独立名称以让组件参数和文档保持一一对应。
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUiHorizontalBarDatasetItem : VueUiVerticalBarDatasetItem;

/// <summary>Vertical bar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiVerticalBarConfig : VueDataUiConfig;

/// <summary>Horizontal bar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHorizontalBarConfig : VueUiVerticalBarConfig;

/// <summary>Sparkline 的时序点。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparklineDatasetItem : Vue.VueProps
{
    /// <summary>
    /// 此数据点对应的时间段或分类标签。
    /// </summary>
    [Description("@#period")]
    public Vue.VueStringNumberValue Period { get; init; } = default!;

    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double? Value { get; init; }
}

/// <summary>Sparkline 的绘制形式。</summary>
[String]
public enum VueUiSparklineType
{
    /// <summary>
    /// 线条形式；上游取值为 “line”。
    /// </summary>
    [Description("@#line")]
    Line,

    /// <summary>
    /// 以条形绘制数据；上游取值为 “bar”。
    /// </summary>
    [Description("@#bar")]
    Bar
}

/// <summary>Sparkline 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparklineConfig : VueDataUiConfig
{
    /// <summary>
    /// 此序列的图形绘制方式，具体选项见枚举成员说明。
    /// </summary>
    [Description("@#type")]
    public VueUiSparklineType? Type { get; init; }

    /// <summary>
    /// 大数据量时的降采样配置，可设置保留数据点的阈值。
    /// </summary>
    [Description("@#downsample")]
    public VueDataUiDownsampleOptions? Downsample { get; init; }
}

/// <summary>Sparkbar 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkbarDatasetItem : VueDataUiDatasetItem
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

/// <summary>Sparkbar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkbarConfig : VueDataUiConfig;

/// <summary>Spark histogram 的单柱 input。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkHistogramDatasetItem : VueDataUiDatasetItem
{
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
    /// 显示在此直方图数据点上的时间文本。
    /// </summary>
    [Description("@#timeLabel")]
    public string? TimeLabel { get; init; }
}

/// <summary>Spark histogram 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkHistogramConfig : VueDataUiConfig;