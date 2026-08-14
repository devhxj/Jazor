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
    [Description("@#light")]
    Light,

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
    [Description("@#threshold")]
    public int? Threshold { get; init; }
}

/// <summary>通用 chart 标题形状。Common title shape shared by the major chart families.</summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiChartTitle : Vue.VueProps
{
    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#subtitle")]
    public VueDataUiChartSubtitle? Subtitle { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#fontSize")]
    public double? FontSize { get; init; }

    [Description("@#bold")]
    public bool? Bold { get; init; }
}

/// <summary>通用 chart subtitle 形状。</summary>
[ECMAScript]
[Description("@#")]
public record VueDataUiChartSubtitle : Vue.VueProps
{
    [Description("@#text")]
    public string? Text { get; init; }

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
    public string? AsString => Value as string;

    public double? AsNumber => Value is double value ? value : default(double?);
}

/// <summary>XY 图表 series 类型。</summary>
[String]
public enum VueUiXySeriesType
{
    [Description("@#bar")]
    Bar,

    [Description("@#line")]
    Line,

    [Description("@#plot")]
    Plot
}

/// <summary>XY coordinate series 的一个点。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyCoordinate : Vue.VueProps
{
    [Description("@#x")]
    public double? X { get; init; }

    [Description("@#y")]
    public double? Y { get; init; }
}

/// <summary>XY series 可为 sequential values 或显式 coordinates。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUiXySeriesValues(double?[], VueUiXyCoordinate[])
{
    public double?[]? AsValues => Value as double?[];

    public VueUiXyCoordinate[]? AsCoordinates => Value as VueUiXyCoordinate[];
}

/// <summary>VueUiXy 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#series")]
    public VueUiXySeriesValues Series { get; init; } = default!;

    [Description("@#type")]
    public VueUiXySeriesType Type { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#useArea")]
    public bool? UseArea { get; init; }

    [Description("@#smooth")]
    public bool? Smooth { get; init; }

    [Description("@#dataLabels")]
    public bool? DataLabels { get; init; }
}

/// <summary>XY config 的稳定公共字段；详细 layout 继续可通过基类字典加 record 扩展。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiXyConfig : VueDataUiConfig
{
    [Description("@#downsample")]
    public VueDataUiDownsampleOptions? Downsample { get; init; }

    [Description("@#usePerformanceMode")]
    public bool? UsePerformanceMode { get; init; }
}

/// <summary>VueUiDonut 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDonutDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }

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
    [Description("@#color")]
    public string Color { get; init; } = string.Empty;

    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

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
    [Description("@#from")]
    public double From { get; init; }

    [Description("@#to")]
    public double To { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#name")]
    public string? Name { get; init; }
}

/// <summary>VueUiGauge 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiGaugeDataset : Vue.VueProps
{
    [Description("@#base")]
    public double? Base { get; init; }

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#series")]
    public VueUiGaugeDatasetSerieItem[] Series { get; init; } = [];

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double? Value { get; init; }
}

/// <summary>Vertical/Horizontal bar 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiVerticalBarDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double? Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

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
    [Description("@#period")]
    public Vue.VueStringNumberValue Period { get; init; } = default!;

    [Description("@#value")]
    public double? Value { get; init; }
}

/// <summary>Sparkline 的绘制形式。</summary>
[String]
public enum VueUiSparklineType
{
    [Description("@#line")]
    Line,

    [Description("@#bar")]
    Bar
}

/// <summary>Sparkline 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparklineConfig : VueDataUiConfig
{
    [Description("@#type")]
    public VueUiSparklineType? Type { get; init; }

    [Description("@#downsample")]
    public VueDataUiDownsampleOptions? Downsample { get; init; }
}

/// <summary>Sparkbar 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkbarDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double? Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#prefix")]
    public string? Prefix { get; init; }

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
    [Description("@#value")]
    public double? Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#timeLabel")]
    public string? TimeLabel { get; init; }
}

/// <summary>Spark histogram 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiSparkHistogramConfig : VueDataUiConfig;
