namespace ECMAScript.VueDataUi;

/// <summary>Radar category authoring item。</summary>
[ECMAScript]
[Description("@#")]
public record VdRadarDatasetCategoryItem : Vue.VueProps
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

/// <summary>Radar series authoring item。</summary>
[ECMAScript]
[Description("@#")]
public record VdRadarDatasetSerieItem : Vue.VueProps
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
    /// 用于对比实际值的目标数值。
    /// </summary>
    [Description("@#target")]
    public double? Target { get; init; }
}

/// <summary>VdRadar 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VdRadarDataset : Vue.VueProps
{
    /// <summary>
    /// 雷达图各维度的分类定义，顺序与每条序列的 values 相对应。
    /// </summary>
    [Description("@#categories")]
    public VdRadarDatasetCategoryItem[] Categories { get; init; } = [];

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VdRadarDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Radar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdRadarConfig : VdConfig;

/// <summary>Waffle 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdWaffleDatasetItem : VdDatasetItem
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
}

/// <summary>Waffle 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdWaffleConfig : VdConfig;

/// <summary>Treemap 的递归 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdTreemapDatasetItem : VdDatasetItem
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
    /// 当前数据项的子节点，用于形成层级或分类细分。
    /// </summary>
    [Description("@#children")]
    public VdTreemapDatasetItem[]? Children { get; init; }

    /// <summary>
    /// 父数据节点的标识，用于关联层级数据。
    /// </summary>
    [Description("@#parentId")]
    public string? ParentId { get; init; }

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Treemap 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdTreemapConfig : VdConfig;

/// <summary>Heatmap 的一行 input。</summary>
[ECMAScript]
[Description("@#")]
public record VdHeatmapDatasetItem : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public Vue.VueStringNumberValue Name { get; init; } = default!;

    /// <summary>
    /// 按分类或时间顺序排列的数据点；与对应的标签数组保持相同顺序。
    /// </summary>
    [Description("@#values")]
    public double?[] Values { get; init; } = [];
}

/// <summary>Heatmap 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdHeatmapConfig : VdConfig;

/// <summary>Scatter point。</summary>
[ECMAScript]
[Description("@#")]
public record VdScatterDatasetValueItem : VdDatasetItem
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

    /// <summary>
    /// 散点的权重，可用于控制数据点的视觉大小。
    /// </summary>
    [Description("@#weight")]
    public double? Weight { get; init; }
}

/// <summary>Scatter series。</summary>
[ECMAScript]
[Description("@#")]
public record VdScatterDatasetItem : VdDatasetItem
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
    public VdScatterDatasetValueItem[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Scatter 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdScatterConfig : VdConfig
{
    /// <summary>
    /// 大数据量时的降采样配置，可设置保留数据点的阈值。
    /// </summary>
    [Description("@#downsample")]
    public VdDownsampleOptions? Downsample { get; init; }

    /// <summary>
    /// 启用面向大数据量的性能模式；组件将使用其性能模式的绘制策略。
    /// </summary>
    [Description("@#usePerformanceMode")]
    public bool? UsePerformanceMode { get; init; }
}

/// <summary>Funnel 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdFunnelDatasetItem : VdDatasetItem
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

/// <summary>Funnel 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdFunnelConfig : VdConfig;

/// <summary>Word cloud 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdWordCloudDatasetItem : VdDatasetItem
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

/// <summary>Word cloud 可接收词项数组或文本 source。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VdWordCloudDataset(VdWordCloudDatasetItem[], string)
{
    /// <summary>
    /// 读取当前值的 VdWordCloudDatasetItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VdWordCloudDatasetItem[]? AsItems => Value as VdWordCloudDatasetItem[];

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsText => Value as string;
}

/// <summary>Word cloud 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdWordCloudConfig : VdConfig;

/// <summary>KPI 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdKpiConfig : VdConfig
{
    /// <summary>
    /// 图表或指标的标题文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

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
    /// 数值变化时启用指标动画。
    /// </summary>
    [Description("@#useAnimation")]
    public bool? UseAnimation { get; init; }

    /// <summary>
    /// 显示数值时保留的小数位数。
    /// </summary>
    [Description("@#valueRounding")]
    public int? ValueRounding { get; init; }
}

/// <summary>VdTable header column。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableDatasetHeaderItem : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 此列的数据类别，用于选择文本或数值等处理方式。
    /// </summary>
    [Description("@#type")]
    public VdTableColumnType Type { get; init; }

    /// <summary>
    /// 是否为此表格列计算平均值。
    /// </summary>
    [Description("@#average")]
    public bool? Average { get; init; }

    /// <summary>
    /// 是否为此表格列计算总和。
    /// </summary>
    [Description("@#sum")]
    public bool? Sum { get; init; }

    /// <summary>
    /// 是否允许按此列排序。
    /// </summary>
    [Description("@#isSort")]
    public bool? IsSort { get; init; }

    /// <summary>
    /// 是否让此列参与表格搜索。
    /// </summary>
    [Description("@#isSearch")]
    public bool? IsSearch { get; init; }
}

/// <summary>VdTable column type literal。</summary>
[String]
public enum VdTableColumnType
{
    /// <summary>
    /// 在当前标题或副标题位置显示的文本。
    /// </summary>
    [Description("@#text")]
    Text,

    /// <summary>
    /// 日期选择；上游取值为 “date”。
    /// </summary>
    [Description("@#date")]
    Date,

    /// <summary>
    /// 数值显示；上游取值为 “numeric”。
    /// </summary>
    [Description("@#numeric")]
    Numeric
}

/// <summary>VdTable 的一行 body cells。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableDatasetBodyItem : Vue.VueProps
{
    /// <summary>
    /// 当前行的单元格值，顺序与 header 一致。
    /// </summary>
    [Description("@#td")]
    public VdCellValue[] Td { get; init; } = [];
}

/// <summary>VdTable 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableDataset : Vue.VueProps
{
    /// <summary>
    /// 表格列的名称、值类型及排序/汇总配置。
    /// </summary>
    [Description("@#header")]
    public VdTableDatasetHeaderItem[] Header { get; init; } = [];

    /// <summary>
    /// 表格正文的行数据，每行单元格顺序应与表头一致。
    /// </summary>
    [Description("@#body")]
    public VdTableDatasetBodyItem[] Body { get; init; } = [];
}

/// <summary>Table 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableConfig : VdConfig
{
    /// <summary>
    /// 表格每页显示的行数。
    /// </summary>
    [Description("@#rowsPerPage")]
    public int? RowsPerPage { get; init; }

    /// <summary>
    /// 表格可显示区域的最大高度。
    /// </summary>
    [Description("@#maxHeight")]
    public double? MaxHeight { get; init; }
}

/// <summary>QuickChart 的 object-form dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdQuickChartDatasetItem : VdDatasetItem;

/// <summary>QuickChart 支持 flat number series、一个 object 或 object series。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VdQuickChartDataset(double?[], VdQuickChartDatasetItem, VdQuickChartDatasetItem[])
{
    /// <summary>
    /// 读取当前值的 double?[] 分支；不属于该分支时返回 null。
    /// </summary>
    public double?[]? AsValues => Value as double?[];

    /// <summary>
    /// 读取当前值的 VdQuickChartDatasetItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VdQuickChartDatasetItem? AsItem => Value as VdQuickChartDatasetItem;

    /// <summary>
    /// 读取当前值的 VdQuickChartDatasetItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VdQuickChartDatasetItem[]? AsItems => Value as VdQuickChartDatasetItem[];
}

/// <summary>QuickChart 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdQuickChartConfig : VdConfig
{
    /// <summary>
    /// 图表或指标的标题文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 是否显示序列图例。
    /// </summary>
    [Description("@#showLegend")]
    public bool? ShowLegend { get; init; }

    /// <summary>
    /// 是否显示鼠标悬停的数据提示。
    /// </summary>
    [Description("@#showTooltip")]
    public bool? ShowTooltip { get; init; }
}

/// <summary>Stackbar 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VdStackbarDatasetItem : VdDatasetItem
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
}

/// <summary>Stackbar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdStackbarConfig : VdConfig;

/// <summary>Stackline 在 stackbar series 上增加 standalone flag。</summary>
[ECMAScript]
[Description("@#")]
public record VdStacklineDatasetItem : VdStackbarDatasetItem
{
    /// <summary>
    /// 将此序列独立显示，不参与同组序列的堆叠。
    /// </summary>
    [Description("@#standalone")]
    public bool? Standalone { get; init; }
}

/// <summary>Stackline 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdStacklineConfig : VdConfig;

/// <summary>Dumbbell 的 start/end dataset row。</summary>
[ECMAScript]
[Description("@#")]
public record VdDumbbellDataset : VdDatasetItem
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 区间或线段的起始数值。
    /// </summary>
    [Description("@#start")]
    public double? Start { get; init; }

    /// <summary>
    /// 区间或线段的结束数值。
    /// </summary>
    [Description("@#end")]
    public double? End { get; init; }
}

/// <summary>Dumbbell 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdDumbbellConfig : VdConfig;

/// <summary>Bullet chart range segment。</summary>
[ECMAScript]
[Description("@#")]
public record VdBulletSegment : Vue.VueProps
{
    /// <summary>
    /// 数据项或序列的显示名称，用于标签、图例和提示内容。
    /// </summary>
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

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
}

/// <summary>VdBullet 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VdBulletDataset : Vue.VueProps
{
    /// <summary>
    /// 此数据项的数值，参与对应图表的长度、位置、面积或刻度计算。
    /// </summary>
    [Description("@#value")]
    public double Value { get; init; }

    /// <summary>
    /// 用于对比实际值的目标数值。
    /// </summary>
    [Description("@#target")]
    public double Target { get; init; }

    /// <summary>
    /// 子弹图的背景区间，每段包含起止数值和颜色。
    /// </summary>
    [Description("@#segments")]
    public VdBulletSegment[] Segments { get; init; } = [];
}

/// <summary>Bullet 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdBulletConfig : VdConfig;

/// <summary>Candlestick config。Dataset rows should be created through <see cref="VdCandlestickData.Ohlc"/>.</summary>
[ECMAScript]
[Description("@#")]
public record VdCandlestickConfig : VdConfig
{
    /// <summary>
    /// 此序列的图形绘制方式，具体选项见枚举成员说明。
    /// </summary>
    [Description("@#type")]
    public VdCandlestickType? Type { get; init; }
}

/// <summary>Candlestick rendering mode。</summary>
[String]
public enum VdCandlestickType
{
    /// <summary>
    /// 使用 OHLC 线条显示开盘、最高、最低和收盘值；上游取值为 “ohlc”。
    /// </summary>
    [Description("@#ohlc")]
    Ohlc,

    /// <summary>
    /// 使用实体蜡烛线显示开盘、收盘、最高和最低值；上游取值为 “candlestick”。
    /// </summary>
    [Description("@#candlestick")]
    Candlestick
}

/// <summary>
/// Candlestick fixed OHLC row factory。C# tuple lowers to a named JS object by design, while upstream
/// requires an array, so this inline boundary deliberately owns the positional runtime shape.
/// </summary>
public static class VdCandlestickData
{
    /// <summary>
    /// 按 [timestamp, open, high, low, close, volume] 顺序构造蜡烛图数据行。
    /// </summary>
    [ECMAScriptInline("[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6]")]
    public extern static VdCellValue[] Ohlc(
        Vue.VueStringNumberValue timestamp,
        double open,
        double high,
        double low,
        double close,
        double volume);
}

/// <summary>Table heatmap 的一行。<c>Values</c> 保留 upstream 允许的 number/string/null cell domain。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableHeatmapDatasetItem : VdDatasetItem
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
    public VdCellValue?[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 数据点或热力单元格的形状；使用当前组件支持的形状值。
    /// </summary>
    [Description("@#shape")]
    public VdTableHeatmapShape? Shape { get; init; }
}

/// <summary>Table heatmap marker shape literal。</summary>
[String]
public enum VdTableHeatmapShape
{
    /// <summary>
    /// 圆形；上游取值为 “circle”。
    /// </summary>
    [Description("@#circle")]
    Circle,

    /// <summary>
    /// 以三角形绘制热力图单元格。
    /// </summary>
    [Description("@#triangle")]
    Triangle,

    /// <summary>
    /// 正方形；上游取值为 “square”。
    /// </summary>
    [Description("@#square")]
    Square,

    /// <summary>
    /// 以菱形绘制热力图单元格。
    /// </summary>
    [Description("@#diamond")]
    Diamond,

    /// <summary>
    /// 以五边形绘制热力图单元格。
    /// </summary>
    [Description("@#pentagon")]
    Pentagon,

    /// <summary>
    /// 以六边形绘制热力图单元格。
    /// </summary>
    [Description("@#hexagon")]
    Hexagon,

    /// <summary>
    /// 以星形绘制热力图单元格。
    /// </summary>
    [Description("@#star")]
    Star
}

/// <summary>Table heatmap 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableHeatmapConfig : VdConfig;

/// <summary>Table sparkline 的一行。每个 row 自带 name、value series 与可选 color。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableSparklineDatasetItem : VdDatasetItem
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

/// <summary>Table sparkline 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VdTableSparklineConfig : VdConfig;