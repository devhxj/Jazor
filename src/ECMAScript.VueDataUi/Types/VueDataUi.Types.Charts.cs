namespace ECMAScript.VueDataUi;

/// <summary>Radar category authoring item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarDatasetCategoryItem : Vue.VueProps
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
public record VueUiRadarDatasetSerieItem : Vue.VueProps
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

/// <summary>VueUiRadar 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarDataset : Vue.VueProps
{
    /// <summary>
    /// 雷达图各维度的分类定义，顺序与每条序列的 values 相对应。
    /// </summary>
    [Description("@#categories")]
    public VueUiRadarDatasetCategoryItem[] Categories { get; init; } = [];

    /// <summary>
    /// 本项包含的数据序列；序列顺序决定与图表分类或时间刻度的对应关系。
    /// </summary>
    [Description("@#series")]
    public VueUiRadarDatasetSerieItem[] Series { get; init; } = [];
}

/// <summary>Radar 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarConfig : VueDataUiConfig;

/// <summary>Waffle 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWaffleDatasetItem : VueDataUiDatasetItem
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
public record VueUiWaffleConfig : VueDataUiConfig;

/// <summary>Treemap 的递归 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTreemapDatasetItem : VueDataUiDatasetItem
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
    public VueUiTreemapDatasetItem[]? Children { get; init; }

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
public record VueUiTreemapConfig : VueDataUiConfig;

/// <summary>Heatmap 的一行 input。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiHeatmapDatasetItem : Vue.VueProps
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
public record VueUiHeatmapConfig : VueDataUiConfig;

/// <summary>Scatter point。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiScatterDatasetValueItem : VueDataUiDatasetItem
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
public record VueUiScatterDatasetItem : VueDataUiDatasetItem
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
    public VueUiScatterDatasetValueItem[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Scatter 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiScatterConfig : VueDataUiConfig
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

/// <summary>Funnel 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiFunnelDatasetItem : VueDataUiDatasetItem
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
public record VueUiFunnelConfig : VueDataUiConfig;

/// <summary>Word cloud 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWordCloudDatasetItem : VueDataUiDatasetItem
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
public readonly union VueUiWordCloudDataset(VueUiWordCloudDatasetItem[], string)
{
    /// <summary>
    /// 读取当前值的 VueUiWordCloudDatasetItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueUiWordCloudDatasetItem[]? AsItems => Value as VueUiWordCloudDatasetItem[];

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsText => Value as string;
}

/// <summary>Word cloud 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiWordCloudConfig : VueDataUiConfig;

/// <summary>KPI 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiKpiConfig : VueDataUiConfig
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

/// <summary>VueUiTable header column。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDatasetHeaderItem : Vue.VueProps
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
    public VueUiTableColumnType Type { get; init; }

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

/// <summary>VueUiTable column type literal。</summary>
[String]
public enum VueUiTableColumnType
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

/// <summary>VueUiTable 的一行 body cells。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDatasetBodyItem : Vue.VueProps
{
    /// <summary>
    /// 当前行的单元格值，顺序与 header 一致。
    /// </summary>
    [Description("@#td")]
    public VueDataUiCellValue[] Td { get; init; } = [];
}

/// <summary>VueUiTable 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDataset : Vue.VueProps
{
    /// <summary>
    /// 表格列的名称、值类型及排序/汇总配置。
    /// </summary>
    [Description("@#header")]
    public VueUiTableDatasetHeaderItem[] Header { get; init; } = [];

    /// <summary>
    /// 表格正文的行数据，每行单元格顺序应与表头一致。
    /// </summary>
    [Description("@#body")]
    public VueUiTableDatasetBodyItem[] Body { get; init; } = [];
}

/// <summary>Table 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableConfig : VueDataUiConfig
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
public record VueUiQuickChartDatasetItem : VueDataUiDatasetItem;

/// <summary>QuickChart 支持 flat number series、一个 object 或 object series。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUiQuickChartDataset(double?[], VueUiQuickChartDatasetItem, VueUiQuickChartDatasetItem[])
{
    /// <summary>
    /// 读取当前值的 double?[] 分支；不属于该分支时返回 null。
    /// </summary>
    public double?[]? AsValues => Value as double?[];

    /// <summary>
    /// 读取当前值的 VueUiQuickChartDatasetItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VueUiQuickChartDatasetItem? AsItem => Value as VueUiQuickChartDatasetItem;

    /// <summary>
    /// 读取当前值的 VueUiQuickChartDatasetItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueUiQuickChartDatasetItem[]? AsItems => Value as VueUiQuickChartDatasetItem[];
}

/// <summary>QuickChart 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiQuickChartConfig : VueDataUiConfig
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
public record VueUiStackbarDatasetItem : VueDataUiDatasetItem
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
public record VueUiStackbarConfig : VueDataUiConfig;

/// <summary>Stackline 在 stackbar series 上增加 standalone flag。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiStacklineDatasetItem : VueUiStackbarDatasetItem
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
public record VueUiStacklineConfig : VueDataUiConfig;

/// <summary>Dumbbell 的 start/end dataset row。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiDumbbellDataset : VueDataUiDatasetItem
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
public record VueUiDumbbellConfig : VueDataUiConfig;

/// <summary>Bullet chart range segment。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBulletSegment : Vue.VueProps
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

/// <summary>VueUiBullet 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBulletDataset : Vue.VueProps
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
    public VueUiBulletSegment[] Segments { get; init; } = [];
}

/// <summary>Bullet 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBulletConfig : VueDataUiConfig;

/// <summary>Candlestick config。Dataset rows should be created through <see cref="VueUiCandlestickData.Ohlc"/>.</summary>
[ECMAScript]
[Description("@#")]
public record VueUiCandlestickConfig : VueDataUiConfig
{
    /// <summary>
    /// 此序列的图形绘制方式，具体选项见枚举成员说明。
    /// </summary>
    [Description("@#type")]
    public VueUiCandlestickType? Type { get; init; }
}

/// <summary>Candlestick rendering mode。</summary>
[String]
public enum VueUiCandlestickType
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
public static class VueUiCandlestickData
{
    /// <summary>
    /// 按 [timestamp, open, high, low, close, volume] 顺序构造蜡烛图数据行。
    /// </summary>
    [ECMAScriptInline("[__arg1, __arg2, __arg3, __arg4, __arg5, __arg6]")]
    public extern static VueDataUiCellValue[] Ohlc(
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
public record VueUiTableHeatmapDatasetItem : VueDataUiDatasetItem
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
    public VueDataUiCellValue?[] Values { get; init; } = [];

    /// <summary>
    /// 此项使用的 CSS 颜色值；可覆盖图表默认调色板分配的颜色。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 数据点或热力单元格的形状；使用当前组件支持的形状值。
    /// </summary>
    [Description("@#shape")]
    public VueUiTableHeatmapShape? Shape { get; init; }
}

/// <summary>Table heatmap marker shape literal。</summary>
[String]
public enum VueUiTableHeatmapShape
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
public record VueUiTableHeatmapConfig : VueDataUiConfig;

/// <summary>Table sparkline 的一行。每个 row 自带 name、value series 与可选 color。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableSparklineDatasetItem : VueDataUiDatasetItem
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
public record VueUiTableSparklineConfig : VueDataUiConfig;