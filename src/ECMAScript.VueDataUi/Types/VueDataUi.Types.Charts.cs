namespace ECMAScript.VueDataUi;

/// <summary>Radar category authoring item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarDatasetCategoryItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#prefix")]
    public string? Prefix { get; init; }

    [Description("@#suffix")]
    public string? Suffix { get; init; }
}

/// <summary>Radar series authoring item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarDatasetSerieItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#target")]
    public double? Target { get; init; }
}

/// <summary>VueUiRadar 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiRadarDataset : Vue.VueProps
{
    [Description("@#categories")]
    public VueUiRadarDatasetCategoryItem[] Categories { get; init; } = [];

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double[] Values { get; init; } = [];

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#children")]
    public VueUiTreemapDatasetItem[]? Children { get; init; }

    [Description("@#parentId")]
    public string? ParentId { get; init; }

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
    [Description("@#name")]
    public Vue.VueStringNumberValue Name { get; init; } = default!;

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#x")]
    public double X { get; init; }

    [Description("@#y")]
    public double Y { get; init; }

    [Description("@#weight")]
    public double? Weight { get; init; }
}

/// <summary>Scatter series。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiScatterDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public VueUiScatterDatasetValueItem[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Scatter 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiScatterConfig : VueDataUiConfig
{
    [Description("@#downsample")]
    public VueDataUiDownsampleOptions? Downsample { get; init; }

    [Description("@#usePerformanceMode")]
    public bool? UsePerformanceMode { get; init; }
}

/// <summary>Funnel 的 dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiFunnelDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Word cloud 可接收词项数组或文本 source。</summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUiWordCloudDataset(VueUiWordCloudDatasetItem[], string)
{
    public VueUiWordCloudDatasetItem[]? AsItems => Value as VueUiWordCloudDatasetItem[];

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
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#prefix")]
    public string? Prefix { get; init; }

    [Description("@#suffix")]
    public string? Suffix { get; init; }

    [Description("@#useAnimation")]
    public bool? UseAnimation { get; init; }

    [Description("@#valueRounding")]
    public int? ValueRounding { get; init; }
}

/// <summary>VueUiTable header column。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDatasetHeaderItem : Vue.VueProps
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#type")]
    public VueUiTableColumnType Type { get; init; }

    [Description("@#average")]
    public bool? Average { get; init; }

    [Description("@#sum")]
    public bool? Sum { get; init; }

    [Description("@#isSort")]
    public bool? IsSort { get; init; }

    [Description("@#isSearch")]
    public bool? IsSearch { get; init; }
}

/// <summary>VueUiTable column type literal。</summary>
[String]
public enum VueUiTableColumnType
{
    [Description("@#text")]
    Text,

    [Description("@#date")]
    Date,

    [Description("@#numeric")]
    Numeric
}

/// <summary>VueUiTable 的一行 body cells。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDatasetBodyItem : Vue.VueProps
{
    [Description("@#td")]
    public VueDataUiCellValue[] Td { get; init; } = [];
}

/// <summary>VueUiTable 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableDataset : Vue.VueProps
{
    [Description("@#header")]
    public VueUiTableDatasetHeaderItem[] Header { get; init; } = [];

    [Description("@#body")]
    public VueUiTableDatasetBodyItem[] Body { get; init; } = [];
}

/// <summary>Table 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableConfig : VueDataUiConfig
{
    [Description("@#rowsPerPage")]
    public int? RowsPerPage { get; init; }

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
    public double?[]? AsValues => Value as double?[];

    public VueUiQuickChartDatasetItem? AsItem => Value as VueUiQuickChartDatasetItem;

    public VueUiQuickChartDatasetItem[]? AsItems => Value as VueUiQuickChartDatasetItem[];
}

/// <summary>QuickChart 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiQuickChartConfig : VueDataUiConfig
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#showLegend")]
    public bool? ShowLegend { get; init; }

    [Description("@#showTooltip")]
    public bool? ShowTooltip { get; init; }
}

/// <summary>Stackbar 的 authoring dataset item。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiStackbarDatasetItem : VueDataUiDatasetItem
{
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#series")]
    public double?[] Series { get; init; } = [];

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#start")]
    public double? Start { get; init; }

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#from")]
    public double From { get; init; }

    [Description("@#to")]
    public double To { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>VueUiBullet 的 dataset。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiBulletDataset : Vue.VueProps
{
    [Description("@#value")]
    public double Value { get; init; }

    [Description("@#target")]
    public double Target { get; init; }

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
    [Description("@#type")]
    public VueUiCandlestickType? Type { get; init; }
}

/// <summary>Candlestick rendering mode。</summary>
[String]
public enum VueUiCandlestickType
{
    [Description("@#ohlc")]
    Ohlc,

    [Description("@#candlestick")]
    Candlestick
}

/// <summary>
/// Candlestick fixed OHLC row factory。C# tuple lowers to a named JS object by design, while upstream
/// requires an array, so this inline boundary deliberately owns the positional runtime shape.
/// </summary>
public static class VueUiCandlestickData
{
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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public VueDataUiCellValue?[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#shape")]
    public VueUiTableHeatmapShape? Shape { get; init; }
}

/// <summary>Table heatmap marker shape literal。</summary>
[String]
public enum VueUiTableHeatmapShape
{
    [Description("@#circle")]
    Circle,

    [Description("@#triangle")]
    Triangle,

    [Description("@#square")]
    Square,

    [Description("@#diamond")]
    Diamond,

    [Description("@#pentagon")]
    Pentagon,

    [Description("@#hexagon")]
    Hexagon,

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
    [Description("@#name")]
    public string Name { get; init; } = string.Empty;

    [Description("@#values")]
    public double?[] Values { get; init; } = [];

    [Description("@#color")]
    public string? Color { get; init; }
}

/// <summary>Table sparkline 的稳定公共 config surface。</summary>
[ECMAScript]
[Description("@#")]
public record VueUiTableSparklineConfig : VueDataUiConfig;
