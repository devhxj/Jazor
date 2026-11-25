using System.ComponentModel;

namespace Jero.Web.App.Components;

/// <summary>ECharts lifecycle and the option shapes used by Analytics and Workspace.</summary>
[ECMAScript("echarts")]
internal static class ECharts
{
    [Description("@#init")]
    public extern static EChartsInstance Init(HTMLElement element);
}

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
    public AnalyticsChartGrid? grid { get; init; }
    public AnalyticsChartTooltip? tooltip { get; init; }
    public AnalyticsChartLegend? legend { get; init; }
    public AnalyticsChartAxis? xAxis { get; init; }
    public AnalyticsChartAxis? yAxis { get; init; }
    public AnalyticsChartRadar? radar { get; init; }
    public AnalyticsChartSeries[] series { get; init; } = [];
}

internal sealed record AnalyticsChartGrid
{
    public string top { get; init; } = "2%";
    public string right { get; init; } = "1%";
    public string bottom { get; init; } = "0";
    public string left { get; init; } = "1%";
    public bool containLabel { get; init; } = true;
}

internal sealed record AnalyticsChartTooltip
{
    public string trigger { get; init; } = "item";
    public AnalyticsChartAxisPointer? axisPointer { get; init; }
}

internal sealed record AnalyticsChartAxisPointer
{
    public AnalyticsChartLineStyle? lineStyle { get; init; }
}

internal sealed record AnalyticsChartLegend
{
    public string bottom { get; init; } = "0";
    public string left { get; init; } = "center";
    public string[] data { get; init; } = [];
}

internal sealed record AnalyticsChartAxis
{
    public string type { get; init; } = "value";
    public string[] data { get; init; } = [];
    public int max { get; init; }
    public int splitNumber { get; init; }
    public bool boundaryGap { get; init; }
    public ChartToggle? axisTick { get; init; }
    public AnalyticsChartSplitLine? splitLine { get; init; }
    public ChartToggle? splitArea { get; init; }
}

internal sealed record ChartToggle
{
    public bool show { get; init; }
}

internal sealed record AnalyticsChartSplitLine
{
    public bool show { get; init; }
    public AnalyticsChartLineStyle lineStyle { get; init; } = new();
}

internal sealed record AnalyticsChartLineStyle
{
    public string color { get; init; } = "#d9e2ec";
    public string type { get; init; } = "solid";
    public int width { get; init; } = 1;
}

internal sealed record AnalyticsChartRadar
{
    public AnalyticsChartRadarIndicator[] indicator { get; init; } = [];
    public string radius { get; init; } = "60%";
    public int splitNumber { get; init; } = 8;
}

internal sealed record AnalyticsChartRadarIndicator
{
    public string name { get; init; } = string.Empty;
}

internal sealed record AnalyticsChartSeries
{
    public string type { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public object data { get; init; } = default!;
    public bool smooth { get; init; }
    public int barMaxWidth { get; init; }
    public int symbolSize { get; init; }
    public bool avoidLabelOverlap { get; init; }
    public string roseType { get; init; } = string.Empty;
    public object? radius { get; init; }
    public string[] center { get; init; } = [];
    public string[] color { get; init; } = [];
    public AnalyticsChartAreaStyle? areaStyle { get; init; }
    public AnalyticsChartItemStyle? itemStyle { get; init; }
    public ChartLabel? label { get; init; }
    public ChartToggle? labelLine { get; init; }
    public ChartEmphasis? emphasis { get; init; }
}

internal sealed record AnalyticsChartAreaStyle
{
    public double opacity { get; init; }
    public int shadowBlur { get; init; }
    public string shadowColor { get; init; } = string.Empty;
    public int shadowOffsetX { get; init; }
    public int shadowOffsetY { get; init; }
}

internal sealed record AnalyticsChartItemStyle
{
    public string color { get; init; } = string.Empty;
    public int borderRadius { get; init; }
    public int borderWidth { get; init; }
}

internal sealed record ChartLabel
{
    public bool show { get; init; }
    public string position { get; init; } = string.Empty;
    public int fontSize { get; init; }
    public string fontWeight { get; init; } = string.Empty;
}

internal sealed record ChartEmphasis
{
    public ChartLabel label { get; init; } = new();
}

internal sealed record AnalyticsChartNamedValue
{
    public string name { get; init; } = string.Empty;
    public int value { get; init; }
    public AnalyticsChartItemStyle? itemStyle { get; init; }
}

internal sealed record AnalyticsChartRadarValue
{
    public string name { get; init; } = string.Empty;
    public int[] value { get; init; } = [];
    public AnalyticsChartItemStyle itemStyle { get; init; } = new();
}

internal sealed record WorkspaceChartOption
{
    public WorkspaceChartTooltip tooltip { get; init; } = new();
    public WorkspaceChartLegend legend { get; init; } = new();
    public WorkspaceChartSeries[] series { get; init; } = [];
}

internal sealed record WorkspaceChartTooltip
{
    public string trigger { get; init; } = "item";
}

internal sealed record WorkspaceChartLegend
{
    public string bottom { get; init; } = "2%";
    public string left { get; init; } = "center";
}

internal sealed record WorkspaceChartSeries
{
    public string type { get; init; } = "pie";
    public string name { get; init; } = string.Empty;
    public string[] radius { get; init; } = [];
    public bool avoidLabelOverlap { get; init; }
    public string[] color { get; init; } = [];
    public WorkspaceChartNamedValue[] data { get; init; } = [];
    public WorkspaceChartItemStyle itemStyle { get; init; } = new();
    public ChartLabel label { get; init; } = new();
    public ChartToggle labelLine { get; init; } = new();
    public ChartEmphasis emphasis { get; init; } = new();
}

internal sealed record WorkspaceChartNamedValue
{
    public string name { get; init; } = string.Empty;
    public int value { get; init; }
}

internal sealed record WorkspaceChartItemStyle
{
    public int borderRadius { get; init; }
    public int borderWidth { get; init; }
}
