using System.ComponentModel;

namespace Jero.Web.Infrastructure.Browser;

/// <summary>分析页和工作台使用的 ECharts 初始化入口。</summary>
[ECMAScript("echarts")]
internal static class ECharts
{
    /// <summary>在已挂载的容器元素上创建图表实例。</summary>
    [Description("@#init")]
    public extern static EChartsInstance Init(HTMLElement element);
}

/// <summary>图表实例，可更新配置、重算尺寸或在页面卸载时释放资源。</summary>
[ECMAScript]
internal sealed class EChartsInstance
{
    [Description("@#setOption")]
    public extern void SetOption(AnalyticsChartOption option);

    [Description("@#setOption")]
    public extern void SetOption(WorkspaceChartOption option);

    [Description("@#resize")]
    public extern void Resize();

    [Description("@#dispose")]
    public extern void Dispose();
}

internal sealed record AnalyticsChartOption
{
    [ECMAScriptName("grid")]
    public AnalyticsChartGrid? Grid { get; init; }
    [ECMAScriptName("tooltip")]
    public AnalyticsChartTooltip? Tooltip { get; init; }
    [ECMAScriptName("legend")]
    public AnalyticsChartLegend? Legend { get; init; }
    [ECMAScriptName("xAxis")]
    public AnalyticsChartAxis? XAxis { get; init; }
    [ECMAScriptName("yAxis")]
    public AnalyticsChartAxis? YAxis { get; init; }
    [ECMAScriptName("radar")]
    public AnalyticsChartRadar? Radar { get; init; }
    [ECMAScriptName("series")]
    public AnalyticsChartSeries[] Series { get; init; } = [];
}

internal sealed record AnalyticsChartGrid
{
    [ECMAScriptName("top")]
    public string Top { get; init; } = "2%";
    [ECMAScriptName("right")]
    public string Right { get; init; } = "1%";
    [ECMAScriptName("bottom")]
    public string Bottom { get; init; } = "0";
    [ECMAScriptName("left")]
    public string Left { get; init; } = "1%";
    [ECMAScriptName("containLabel")]
    public bool ContainLabel { get; init; } = true;
}

internal sealed record AnalyticsChartTooltip
{
    [ECMAScriptName("trigger")]
    public string Trigger { get; init; } = "item";
    [ECMAScriptName("axisPointer")]
    public AnalyticsChartAxisPointer? AxisPointer { get; init; }
}

internal sealed record AnalyticsChartAxisPointer
{
    [ECMAScriptName("lineStyle")]
    public AnalyticsChartLineStyle? LineStyle { get; init; }
}

internal sealed record AnalyticsChartLegend
{
    [ECMAScriptName("bottom")]
    public string Bottom { get; init; } = "0";
    [ECMAScriptName("left")]
    public string Left { get; init; } = "center";
    [ECMAScriptName("data")]
    public string[] Data { get; init; } = [];
}

internal sealed record AnalyticsChartAxis
{
    [ECMAScriptName("type")]
    public string Type { get; init; } = "value";
    [ECMAScriptName("data")]
    public string[] Data { get; init; } = [];
    [ECMAScriptName("max")]
    public int Max { get; init; }
    [ECMAScriptName("splitNumber")]
    public int SplitNumber { get; init; }
    [ECMAScriptName("boundaryGap")]
    public bool BoundaryGap { get; init; }
    [ECMAScriptName("axisTick")]
    public ChartToggle? AxisTick { get; init; }
    [ECMAScriptName("splitLine")]
    public AnalyticsChartSplitLine? SplitLine { get; init; }
    [ECMAScriptName("splitArea")]
    public ChartToggle? SplitArea { get; init; }
}

internal sealed record ChartToggle
{
    [ECMAScriptName("show")]
    public bool Show { get; init; }
}

internal sealed record AnalyticsChartSplitLine
{
    [ECMAScriptName("show")]
    public bool Show { get; init; }
    [ECMAScriptName("lineStyle")]
    public AnalyticsChartLineStyle LineStyle { get; init; } = new();
}

internal sealed record AnalyticsChartLineStyle
{
    [ECMAScriptName("color")]
    public string Color { get; init; } = "#d9e2ec";
    [ECMAScriptName("type")]
    public string Type { get; init; } = "solid";
    [ECMAScriptName("width")]
    public int Width { get; init; } = 1;
}

internal sealed record AnalyticsChartRadar
{
    [ECMAScriptName("indicator")]
    public AnalyticsChartRadarIndicator[] Indicator { get; init; } = [];
    [ECMAScriptName("radius")]
    public string Radius { get; init; } = "60%";
    [ECMAScriptName("splitNumber")]
    public int SplitNumber { get; init; } = 8;
}

internal sealed record AnalyticsChartRadarIndicator
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
}

internal sealed record AnalyticsChartSeries
{
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("data")]
    public object Data { get; init; } = default!;
    [ECMAScriptName("smooth")]
    public bool Smooth { get; init; }
    [ECMAScriptName("barMaxWidth")]
    public int BarMaxWidth { get; init; }
    [ECMAScriptName("symbolSize")]
    public int SymbolSize { get; init; }
    [ECMAScriptName("avoidLabelOverlap")]
    public bool AvoidLabelOverlap { get; init; }
    [ECMAScriptName("roseType")]
    public string RoseType { get; init; } = string.Empty;
    [ECMAScriptName("radius")]
    public object? Radius { get; init; }
    [ECMAScriptName("center")]
    public string[] Center { get; init; } = [];
    [ECMAScriptName("color")]
    public string[] Color { get; init; } = [];
    [ECMAScriptName("areaStyle")]
    public AnalyticsChartAreaStyle? AreaStyle { get; init; }
    [ECMAScriptName("itemStyle")]
    public AnalyticsChartItemStyle? ItemStyle { get; init; }
    [ECMAScriptName("label")]
    public ChartLabel? Label { get; init; }
    [ECMAScriptName("labelLine")]
    public ChartToggle? LabelLine { get; init; }
    [ECMAScriptName("emphasis")]
    public ChartEmphasis? Emphasis { get; init; }
}

internal sealed record AnalyticsChartAreaStyle
{
    [ECMAScriptName("opacity")]
    public double Opacity { get; init; }
    [ECMAScriptName("shadowBlur")]
    public int ShadowBlur { get; init; }
    [ECMAScriptName("shadowColor")]
    public string ShadowColor { get; init; } = string.Empty;
    [ECMAScriptName("shadowOffsetX")]
    public int ShadowOffsetX { get; init; }
    [ECMAScriptName("shadowOffsetY")]
    public int ShadowOffsetY { get; init; }
}

internal sealed record AnalyticsChartItemStyle
{
    [ECMAScriptName("color")]
    public string Color { get; init; } = string.Empty;
    [ECMAScriptName("borderRadius")]
    public int BorderRadius { get; init; }
    [ECMAScriptName("borderWidth")]
    public int BorderWidth { get; init; }
}

internal sealed record ChartLabel
{
    [ECMAScriptName("show")]
    public bool Show { get; init; }
    [ECMAScriptName("position")]
    public string Position { get; init; } = string.Empty;
    [ECMAScriptName("fontSize")]
    public int FontSize { get; init; }
    [ECMAScriptName("fontWeight")]
    public string FontWeight { get; init; } = string.Empty;
}

internal sealed record ChartEmphasis
{
    [ECMAScriptName("label")]
    public ChartLabel Label { get; init; } = new();
}

internal sealed record AnalyticsChartNamedValue
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public int Value { get; init; }
    [ECMAScriptName("itemStyle")]
    public AnalyticsChartItemStyle? ItemStyle { get; init; }
}

internal sealed record AnalyticsChartRadarValue
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public int[] Value { get; init; } = [];
    [ECMAScriptName("itemStyle")]
    public AnalyticsChartItemStyle ItemStyle { get; init; } = new();
}

internal sealed record WorkspaceChartOption
{
    [ECMAScriptName("tooltip")]
    public WorkspaceChartTooltip Tooltip { get; init; } = new();
    [ECMAScriptName("legend")]
    public WorkspaceChartLegend Legend { get; init; } = new();
    [ECMAScriptName("series")]
    public WorkspaceChartSeries[] Series { get; init; } = [];
}

internal sealed record WorkspaceChartTooltip
{
    [ECMAScriptName("trigger")]
    public string Trigger { get; init; } = "item";
}

internal sealed record WorkspaceChartLegend
{
    [ECMAScriptName("bottom")]
    public string Bottom { get; init; } = "2%";
    [ECMAScriptName("left")]
    public string Left { get; init; } = "center";
}

internal sealed record WorkspaceChartSeries
{
    [ECMAScriptName("type")]
    public string Type { get; init; } = "pie";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("radius")]
    public string[] Radius { get; init; } = [];
    [ECMAScriptName("avoidLabelOverlap")]
    public bool AvoidLabelOverlap { get; init; }
    [ECMAScriptName("color")]
    public string[] Color { get; init; } = [];
    [ECMAScriptName("data")]
    public WorkspaceChartNamedValue[] Data { get; init; } = [];
    [ECMAScriptName("itemStyle")]
    public WorkspaceChartItemStyle ItemStyle { get; init; } = new();
    [ECMAScriptName("label")]
    public ChartLabel Label { get; init; } = new();
    [ECMAScriptName("labelLine")]
    public ChartToggle LabelLine { get; init; } = new();
    [ECMAScriptName("emphasis")]
    public ChartEmphasis Emphasis { get; init; } = new();
}

internal sealed record WorkspaceChartNamedValue
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public int Value { get; init; }
}

internal sealed record WorkspaceChartItemStyle
{
    [ECMAScriptName("borderRadius")]
    public int BorderRadius { get; init; }
    [ECMAScriptName("borderWidth")]
    public int BorderWidth { get; init; }
}
